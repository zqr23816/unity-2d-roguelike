using UnityEngine;

/// <summary>唯一的实体创建入口；实例化 Prefab 后只注入本局依赖和动态数值。</summary>
public sealed class RoguelikeEntityFactory
{
    private readonly RoguelikePrefabCatalog catalog;
    private readonly GameBalanceSettings settings;

    public RoguelikeEntityFactory(RoguelikePrefabCatalog prefabCatalog, GameBalanceSettings balanceSettings)
    {
        catalog = prefabCatalog;
        settings = balanceSettings;
        if (catalog == null || !catalog.IsComplete)
            throw new System.InvalidOperationException("RoguelikePrefabCatalog 未配置完整，请运行 Roguelike/生成菜单、Prefab 与主场景。");
    }

    public PlayerController CreatePlayer(Vector2 position)
    {
        GameObject owner = Object.Instantiate(catalog.PlayerPrefab, position, Quaternion.identity);
        owner.name = "Player";
        PlayerController controller = Require<PlayerController>(owner);
        controller.Configure(settings);
        return controller;
    }

    public EnemyController CreateEnemy(Vector2 position, int roomDepth, int floor, bool boss,
        int spawnIndex, PlayerController player, DungeonGenerator dungeon)
    {
        GameObject owner = Object.Instantiate(catalog.EnemyPrefab, position, Quaternion.identity);
        PixelCharacterAnimator.CharacterKind kind;
        if (boss) kind = PixelCharacterAnimator.CharacterKind.BigZombie;
        else if (spawnIndex % 3 == 1) kind = PixelCharacterAnimator.CharacterKind.OrcWarrior;
        else if (spawnIndex % 3 == 2) kind = PixelCharacterAnimator.CharacterKind.BigZombie;
        else kind = PixelCharacterAnimator.CharacterKind.Goblin;

        owner.name = boss ? "Boss - Big Zombie" : "Enemy - " + kind;
        Require<PixelCharacterAnimator>(owner).Configure(kind);
        CircleCollider2D collider = Require<CircleCollider2D>(owner);
        collider.radius = boss ? 0.62f : 0.47f;
        EnemyController enemy = Require<EnemyController>(owner);
        enemy.Configure(player, dungeon, roomDepth, floor, boss, settings);
        return enemy;
    }

    public ExperienceOrb CreateExperienceOrb(Vector2 position, PlayerController player, int value)
    {
        GameObject owner = Object.Instantiate(catalog.ExperienceOrbPrefab, position, Quaternion.identity);
        owner.name = "Experience Orb";
        ExperienceOrb orb = Require<ExperienceOrb>(owner);
        orb.Configure(player, value);
        return orb;
    }

    public WeaponPickup CreateWeaponPickup(Vector2 position, WeaponId weapon)
    {
        GameObject owner = Object.Instantiate(catalog.WeaponPickupPrefab, position, Quaternion.identity);
        owner.name = "Boss Weapon Drop - " + weapon;
        WeaponPickup pickup = Require<WeaponPickup>(owner);
        pickup.Configure(weapon, settings);
        return pickup;
    }

    private static T Require<T>(GameObject owner) where T : Component
    {
        T component = owner.GetComponent<T>();
        if (component == null) throw new MissingComponentException($"Prefab {owner.name} 缺少组件 {typeof(T).Name}");
        return component;
    }
}
