using System;
using UnityEngine;

/// <summary>负责敌人波次、经验掉落、Boss 奖励和通关条件。</summary>
public sealed class EncounterDirector
{
    private readonly RoguelikeEntityFactory factory;
    private readonly GameBalanceSettings settings;
    private readonly PlayerController player;
    private readonly DungeonGenerator dungeon;
    private readonly int floor;

    public EncounterDirector(RoguelikeEntityFactory entityFactory, GameBalanceSettings balanceSettings,
        PlayerController target, DungeonGenerator generator, int currentFloor)
    {
        factory = entityFactory;
        settings = balanceSettings;
        player = target;
        dungeon = generator;
        floor = currentFloor;
    }

    public int EnemiesAlive { get; private set; }
    public int Kills { get; private set; }
    public bool IsAwaitingBossReward { get; private set; }
    public WeaponPickup NearbyWeapon { get; private set; }
    public event Action Completed;
    public event Action<string> StatusChanged;

    public void SpawnAll()
    {
        for (int i = 0; i < dungeon.EnemySpawns.Count; i++)
        {
            bool boss = i == dungeon.EnemySpawns.Count - 1;
            factory.CreateEnemy(dungeon.EnemySpawns[i], Mathf.Max(1, (i + 1) / 4), floor,
                boss, i, player, dungeon);
            EnemiesAlive++;
        }
    }

    public void NotifyEnemyKilled(Vector2 position, bool wasBoss)
    {
        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
        Kills++;
        for (int i = 0; i < 3; i++)
            factory.CreateExperienceOrb(position + UnityEngine.Random.insideUnitCircle * 0.45f, player, 2);
        if (wasBoss) SpawnBossWeapon(position);
        TryComplete();
    }

    public void SetNearbyWeapon(WeaponPickup pickup) { NearbyWeapon = pickup; }

    public void ResolveBossReward(bool equipped, string weaponName)
    {
        IsAwaitingBossReward = false;
        NearbyWeapon = null;
        StatusChanged?.Invoke(equipped ? "已装备：" + weaponName : "保留当前武器");
        TryComplete();
    }

    private void SpawnBossWeapon(Vector2 position)
    {
        if (settings.bossDropPool == null || settings.bossDropPool.Count == 0) return;
        WeaponId selected = settings.bossDropPool[UnityEngine.Random.Range(0, settings.bossDropPool.Count)];
        factory.CreateWeaponPickup(position, selected);
        IsAwaitingBossReward = true;
        StatusChanged?.Invoke("Boss 掉落了新武器");
    }

    private void TryComplete()
    {
        if (EnemiesAlive == 0 && !IsAwaitingBossReward) Completed?.Invoke();
    }
}
