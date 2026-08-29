using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>集中管理可在 Inspector 中调节的玩家、敌人、Boss 与武器参数。</summary>
[CreateAssetMenu(fileName = "GameBalanceSettings", menuName = "Roguelike/数值配置")]
public sealed class GameBalanceSettings : ScriptableObject
{
    [Serializable]
    public sealed class WeaponTuning
    {
        public WeaponId id;
        public string displayName;
        public string resourcePath;
        public int damageBonus;
        [Min(0.1f)] public float rangeMultiplier = 1f;
        [Min(0.1f)] public float cooldownMultiplier = 1f;
    }

    [Header("玩家基础参数")]
    [Min(1)] public int playerMaxHealth = 100;
    [Min(0.1f)] public float playerMoveSpeed = 5f;
    [Min(1)] public int playerBaseDamage = 20;
    [Min(0.1f)] public float playerAttackRange = 1.25f;
    [Min(0.05f)] public float playerAttackCooldown = 0.35f;
    [Min(0f)] public float playerInvulnerability = 0.65f;

    [Header("普通敌人参数")]
    [Min(1)] public int enemyBaseHealth = 35;
    [Min(0)] public int enemyHealthPerDepth = 8;
    [Min(0.1f)] public float enemyMoveSpeed = 2.1f;
    [Min(0f)] public float enemySpeedPerDepth = 0.08f;
    [Min(1)] public int enemyContactDamage = 8;
    [Min(0)] public int enemyDamagePerDepth = 1;
    [Min(0.1f)] public float enemyAttackRange = 1.15f;
    [Min(0.1f)] public float enemyChaseRange = 8f;
    [Min(0.1f)] public float enemyAttackCooldown = 0.9f;

    [Header("敌人随层数与玩家等级成长")]
    [Min(0f)] public float enemyHealthGrowthPerFloor = 0.22f;
    [Min(0f)] public float enemyHealthGrowthPerPlayerLevel = 0.06f;
    [Min(0f)] public float enemyDamageGrowthPerFloor = 0.15f;
    [Min(0f)] public float enemyDamageGrowthPerPlayerLevel = 0.04f;
    [Min(0)] public int enemyBaseDefense = 0;
    [Min(0f)] public float enemyDefensePerFloor = 1f;
    [Min(0f)] public float enemyDefensePerPlayerLevel = 0.5f;

    [Header("关底 Boss 参数")]
    [Min(1f)] public float bossHealthMultiplier = 4f;
    [Min(1f)] public float bossDamageMultiplier = 1.6f;
    [Min(0.1f)] public float bossScale = 1.35f;

    [Header("武器配置")]
    public WeaponId initialWeapon = WeaponId.Chopper;
    public List<WeaponId> bossDropPool = new List<WeaponId>();
    public List<WeaponTuning> weapons = new List<WeaponTuning>();

    public WeaponTuning GetWeapon(WeaponId id)
    {
        return weapons.Find(item => item.id == id);
    }

    public void ResetToRecommendedDefaults()
    {
        bossDropPool = new List<WeaponId>
        {
            WeaponId.SteelSword,
            WeaponId.GoldenSword,
            WeaponId.Hammer,
            WeaponId.SilverKatana
        };
        weapons = new List<WeaponTuning>
        {
            NewWeapon(WeaponId.Chopper, "劈刀", "Art/0x72/DungeonTilesetI/Weapons/weapon_chopper", 4, 1f, 1f),
            NewWeapon(WeaponId.SteelSword, "钢剑", "Art/0x72/DungeonTilesetI/Weapons/weapon_sword_steel", 10, 1.12f, 0.92f),
            NewWeapon(WeaponId.GoldenSword, "黄金剑", "Art/0x72/DungeonTilesetI/Weapons/weapon_sword_golden", 16, 1.08f, 1.05f),
            NewWeapon(WeaponId.Hammer, "战锤", "Art/0x72/DungeonTilesetI/Weapons/weapon_hammer", 22, 1.2f, 1.3f),
            NewWeapon(WeaponId.SilverKatana, "银色武士刀", "Art/0x72/DungeonTilesetI/Weapons/weapon_katana_silver", 12, 1.28f, 0.8f)
        };
    }

    private static WeaponTuning NewWeapon(WeaponId id, string displayName, string path, int damage, float range, float cooldown)
    {
        return new WeaponTuning
        {
            id = id,
            displayName = displayName,
            resourcePath = path,
            damageBonus = damage,
            rangeMultiplier = range,
            cooldownMultiplier = cooldown
        };
    }
}
