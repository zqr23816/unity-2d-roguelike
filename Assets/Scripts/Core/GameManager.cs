using System.Collections.Generic;
using UnityEngine;

/// <summary>局内协调器：组合会话、实体工厂、战斗波次和成长系统，并向 UI 提供稳定接口。</summary>
public sealed class GameManager : MonoBehaviour
{
    public enum UpgradeType { MaxHealth, Damage, MoveSpeed, AttackSpeed, AttackRange, Recover }
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameBalanceSettings balanceSettings;
    [SerializeField] private RoguelikePrefabCatalog prefabCatalog;

    private PlayerController player;
    private RunSession session;
    private RunProgressionSystem progression;
    private EncounterDirector encounter;
    private bool gameOver;
    private bool victory;
    private float statusUntil;

    public PlayerController Player => player;
    public GameBalanceSettings BalanceSettings => balanceSettings;
    public bool IsInputBlocked => gameOver || victory || IsChoosingUpgrade;
    public bool IsGameOver => gameOver;
    public bool IsVictory => victory;
    public bool IsChoosingUpgrade => progression != null && progression.IsChoosing;
    public bool IsAwaitingBossReward => encounter != null && encounter.IsAwaitingBossReward;
    public int EnemiesAlive => encounter != null ? encounter.EnemiesAlive : 0;
    public int Kills => encounter != null ? encounter.Kills : 0;
    public int Seed => session != null ? session.Seed : 0;
    public int Floor => session != null ? session.Floor : 1;
    public string StatusMessage { get; private set; }
    public WeaponPickup NearbyWeapon => encounter != null ? encounter.NearbyWeapon : null;
    public IReadOnlyList<UpgradeType> UpgradeOptions => progression != null ? progression.Options : EmptyUpgrades;
    private static readonly IReadOnlyList<UpgradeType> EmptyUpgrades = new UpgradeType[0];

    private void Awake()
    {
        Instance = this;
        if (balanceSettings == null) balanceSettings = Resources.Load<GameBalanceSettings>("Settings/GameBalanceSettings");
        if (prefabCatalog == null) prefabCatalog = Resources.Load<RoguelikePrefabCatalog>("Settings/RoguelikePrefabCatalog");
    }

    private void Start()
    {
        if (balanceSettings == null || prefabCatalog == null || !prefabCatalog.IsComplete)
        {
            Debug.LogError("缺少数值配置或 Prefab 目录，请运行 Roguelike/生成菜单、Prefab 与主场景。");
            enabled = false;
            return;
        }

        Time.timeScale = 1f;
        session = new RunSession();
        session.Initialize();
        DungeonGenerator generator = gameObject.AddComponent<DungeonGenerator>();
        generator.Generate(session.Seed);

        RoguelikeEntityFactory factory = new RoguelikeEntityFactory(prefabCatalog, balanceSettings);
        player = factory.CreatePlayer(generator.PlayerSpawn);
        if (session.InitialSave != null)
        {
            player.Restore(session.InitialSave);
            ShowStatus("已读取角色成长存档");
        }

        progression = new RunProgressionSystem(player);
        encounter = new EncounterDirector(factory, balanceSettings, player, generator, session.Floor);
        encounter.Completed += CompleteFloor;
        encounter.StatusChanged += ShowStatus;
        encounter.SpawnAll();

        CameraFollow cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        cameraFollow?.SetTarget(player.transform);
        if (GetComponent<RoguelikeHUD>() == null) gameObject.AddComponent<RoguelikeHUD>();
    }

    private void Update()
    {
        if (gameOver && Input.GetKeyDown(KeyCode.R)) session.Restart();
        if (victory && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.N))) session.Advance(player);
        if (Input.GetKeyDown(KeyCode.F5) && player != null && !gameOver)
        {
            session.Save(player);
            ShowStatus("角色成长已保存");
        }
        if (Input.GetKeyDown(KeyCode.Escape)) RunSession.ReturnToMenu();
        if (IsChoosingUpgrade)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyUpgrade(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyUpgrade(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyUpgrade(2);
        }
        if (!string.IsNullOrEmpty(StatusMessage) && Time.unscaledTime >= statusUntil) StatusMessage = null;
    }

    public void NotifyEnemyKilled(Vector2 position, bool wasBoss) => encounter?.NotifyEnemyKilled(position, wasBoss);
    public void SetNearbyWeapon(WeaponPickup pickup) => encounter?.SetNearbyWeapon(pickup);
    public void ResolveBossReward(bool equipped, string weaponName) => encounter?.ResolveBossReward(equipped, weaponName);

    public void NotifyPlayerDied()
    {
        gameOver = true;
        Time.timeScale = 0f;
    }

    public void BeginUpgradeChoice() => progression?.Begin(gameOver || victory);
    public void ApplyUpgrade(int index) => progression?.Apply(index);

    private void CompleteFloor()
    {
        victory = true;
        Time.timeScale = 0f;
    }

    private void ShowStatus(string message)
    {
        StatusMessage = message;
        statusUntil = Time.unscaledTime + 3f;
    }

    public static string GetUpgradeName(UpgradeType type) => RunProgressionSystem.GetName(type);
}
