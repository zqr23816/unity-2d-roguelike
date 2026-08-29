using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>组织一局游戏的生成、刷怪、经验、武器、存档、升级与结算流程。</summary>
public sealed class GameManager : MonoBehaviour
{
    public enum UpgradeType { MaxHealth, Damage, MoveSpeed, AttackSpeed, AttackRange, Recover }
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameBalanceSettings balanceSettings;
    private readonly List<UpgradeType> upgradeOptions = new List<UpgradeType>(3);
    private PlayerController player;
    private bool gameOver;
    private bool victory;
    private bool choosingUpgrade;
    private bool awaitingBossReward;
    private int enemiesAlive;
    private int kills;
    private float statusUntil;

    public PlayerController Player => player;
    public GameBalanceSettings BalanceSettings => balanceSettings;
    public bool IsInputBlocked => gameOver || victory || choosingUpgrade;
    public bool IsGameOver => gameOver;
    public bool IsVictory => victory;
    public bool IsChoosingUpgrade => choosingUpgrade;
    public bool IsAwaitingBossReward => awaitingBossReward;
    public int EnemiesAlive => enemiesAlive;
    public int Kills => kills;
    public int Seed { get; private set; }
    public int Floor { get; private set; } = 1;
    public string StatusMessage { get; private set; }
    public WeaponPickup NearbyWeapon { get; private set; }
    public IReadOnlyList<UpgradeType> UpgradeOptions => upgradeOptions;

    private void Awake()
    {
        Instance = this;
        if (balanceSettings == null) balanceSettings = Resources.Load<GameBalanceSettings>("Settings/GameBalanceSettings");
    }

    private void Start()
    {
        if (balanceSettings == null)
        {
            Debug.LogError("缺少 GameBalanceSettings，请运行 Roguelike/生成菜单与主场景。");
            enabled = false;
            return;
        }

        Time.timeScale = 1f;
        RunSaveData save = RunSaveSystem.ConsumeLoadRequest();
        Floor = save != null ? Mathf.Max(1, save.floor) : 1;
        Seed = save != null && save.seed != 0 ? save.seed : unchecked((int)DateTime.Now.Ticks);
        UnityEngine.Random.InitState(Seed);

        DungeonGenerator generator = gameObject.AddComponent<DungeonGenerator>();
        generator.Generate(Seed);
        player = CreatePlayer(generator.PlayerSpawn);
        if (save != null)
        {
            player.Restore(save);
            ShowStatus("已读取角色成长存档");
        }

        Camera.main.GetComponent<CameraFollow>().SetTarget(player.transform);
        for (int i = 0; i < generator.EnemySpawns.Count; i++)
        {
            bool boss = i == generator.EnemySpawns.Count - 1;
            CreateEnemy(generator.EnemySpawns[i], Mathf.Max(1, (i + 1) / 4), boss, generator);
        }
        gameObject.AddComponent<RoguelikeHUD>();
    }

    private void Update()
    {
        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            RunSaveSystem.RequestNewGame();
            SceneManager.LoadScene("Main");
        }
        if (victory && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.N)))
        {
            AdvanceToNextFloor();
        }
        if (Input.GetKeyDown(KeyCode.F5) && player != null && !gameOver)
        {
            RunSaveSystem.Save(player.CreateSaveData(Seed, Floor));
            ShowStatus("角色成长已保存");
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Menu");
        }
        if (choosingUpgrade)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyUpgrade(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyUpgrade(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyUpgrade(2);
        }
        if (!string.IsNullOrEmpty(StatusMessage) && Time.unscaledTime >= statusUntil) StatusMessage = null;
    }

    private PlayerController CreatePlayer(Vector2 position)
    {
        GameObject owner = new GameObject("Player");
        owner.transform.position = position;
        SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 3;
        PixelCharacterAnimator animator = owner.AddComponent<PixelCharacterAnimator>();
        animator.Configure(PixelCharacterAnimator.CharacterKind.KnightFemale);
        owner.AddComponent<Rigidbody2D>().gravityScale = 0f;
        owner.AddComponent<CircleCollider2D>().radius = 0.48f;
        PlayerController controller = owner.AddComponent<PlayerController>();
        controller.Configure(balanceSettings);
        return controller;
    }

    private void CreateEnemy(Vector2 position, int depth, bool boss, DungeonGenerator generator)
    {
        PixelCharacterAnimator.CharacterKind kind;
        if (boss) kind = PixelCharacterAnimator.CharacterKind.BigZombie;
        else if (enemiesAlive % 3 == 1) kind = PixelCharacterAnimator.CharacterKind.OrcWarrior;
        else if (enemiesAlive % 3 == 2) kind = PixelCharacterAnimator.CharacterKind.BigZombie;
        else kind = PixelCharacterAnimator.CharacterKind.Goblin;

        GameObject owner = new GameObject("Enemy - " + kind);
        owner.transform.position = position;
        owner.AddComponent<SpriteRenderer>().sortingOrder = 2;
        PixelCharacterAnimator animator = owner.AddComponent<PixelCharacterAnimator>();
        animator.Configure(kind);
        owner.AddComponent<Rigidbody2D>().gravityScale = 0f;
        owner.AddComponent<CircleCollider2D>().radius = boss ? 0.62f : 0.47f;
        EnemyController enemy = owner.AddComponent<EnemyController>();
        enemy.Configure(player, generator, depth, Floor, boss, balanceSettings);
        enemiesAlive++;
    }

    public void NotifyEnemyKilled(Vector2 position, bool wasBoss)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        kills++;
        for (int i = 0; i < 3; i++) CreateExperienceOrb(position + UnityEngine.Random.insideUnitCircle * 0.45f, 2);
        if (wasBoss)
        {
            SpawnBossWeapon(position);
        }
        TryCompleteRun();
    }

    private void SpawnBossWeapon(Vector2 position)
    {
        if (balanceSettings.bossDropPool == null || balanceSettings.bossDropPool.Count == 0) return;
        WeaponId selected = balanceSettings.bossDropPool[UnityEngine.Random.Range(0, balanceSettings.bossDropPool.Count)];
        GameObject owner = new GameObject("Boss Weapon Drop - " + selected);
        owner.transform.position = position;
        owner.AddComponent<SpriteRenderer>();
        owner.AddComponent<CircleCollider2D>();
        WeaponPickup pickup = owner.AddComponent<WeaponPickup>();
        pickup.Configure(selected, balanceSettings);
        awaitingBossReward = true;
        ShowStatus("Boss 掉落了新武器");
    }

    public void SetNearbyWeapon(WeaponPickup pickup) { NearbyWeapon = pickup; }

    public void ResolveBossReward(bool equipped, string weaponName)
    {
        awaitingBossReward = false;
        NearbyWeapon = null;
        ShowStatus(equipped ? "已装备：" + weaponName : "保留当前武器");
        TryCompleteRun();
    }

    private void TryCompleteRun()
    {
        if (enemiesAlive == 0 && !awaitingBossReward)
        {
            victory = true;
            Time.timeScale = 0f;
        }
    }

    private void AdvanceToNextFloor()
    {
        Time.timeScale = 1f;
        RunSaveData nextFloor = player.CreateSaveData(0, Floor + 1);
        RunSaveSystem.Save(nextFloor);
        RunSaveSystem.RequestNextFloor(nextFloor);
        SceneManager.LoadScene("Main");
    }

    private void CreateExperienceOrb(Vector2 position, int value)
    {
        GameObject owner = new GameObject("Experience Orb");
        owner.transform.position = position;
        RuntimeSpriteFactory.AddRenderer(owner, new Color(0.3f, 1f, 0.62f), 2, Vector2.one * 0.24f);
        CircleCollider2D collider = owner.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        ExperienceOrb orb = owner.AddComponent<ExperienceOrb>();
        orb.Configure(player, value);
    }

    public void NotifyPlayerDied() { gameOver = true; Time.timeScale = 0f; }

    public void BeginUpgradeChoice()
    {
        if (gameOver || victory) return;
        choosingUpgrade = true;
        upgradeOptions.Clear();
        while (upgradeOptions.Count < 3)
        {
            UpgradeType option = (UpgradeType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(UpgradeType)).Length);
            if (!upgradeOptions.Contains(option)) upgradeOptions.Add(option);
        }
        Time.timeScale = 0f;
    }

    public void ApplyUpgrade(int index)
    {
        if (!choosingUpgrade || index < 0 || index >= upgradeOptions.Count) return;
        switch (upgradeOptions[index])
        {
            case UpgradeType.MaxHealth: player.IncreaseMaxHealth(25); break;
            case UpgradeType.Damage: player.IncreaseDamage(8); break;
            case UpgradeType.MoveSpeed: player.IncreaseMoveSpeed(0.6f); break;
            case UpgradeType.AttackSpeed: player.ImproveAttackSpeed(0.84f); break;
            case UpgradeType.AttackRange: player.IncreaseAttackRange(0.22f); break;
            case UpgradeType.Recover: player.Heal(45); break;
        }
        choosingUpgrade = false;
        Time.timeScale = 1f;
    }

    private void ShowStatus(string message)
    {
        StatusMessage = message;
        statusUntil = Time.unscaledTime + 3f;
    }

    public static string GetUpgradeName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.MaxHealth: return "生命强化：最大生命 +25";
            case UpgradeType.Damage: return "锋利武器：基础伤害 +8";
            case UpgradeType.MoveSpeed: return "迅捷步伐：移速 +0.6";
            case UpgradeType.AttackSpeed: return "战斗节奏：攻击更快";
            case UpgradeType.AttackRange: return "长柄技巧：基础攻击范围增加";
            case UpgradeType.Recover: return "急救补给：恢复 45 生命";
            default: return type.ToString();
        }
    }
}
