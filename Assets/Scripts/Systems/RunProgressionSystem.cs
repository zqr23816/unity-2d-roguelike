using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>负责升级候选生成与属性应用，不关心场景、存档和敌人。</summary>
public sealed class RunProgressionSystem
{
    private readonly List<GameManager.UpgradeType> options = new List<GameManager.UpgradeType>(3);
    private readonly PlayerController player;

    public RunProgressionSystem(PlayerController target) { player = target; }
    public bool IsChoosing { get; private set; }
    public IReadOnlyList<GameManager.UpgradeType> Options => options;

    public void Begin(bool blocked)
    {
        if (blocked || IsChoosing) return;
        IsChoosing = true;
        options.Clear();
        int count = Enum.GetValues(typeof(GameManager.UpgradeType)).Length;
        while (options.Count < 3)
        {
            GameManager.UpgradeType option = (GameManager.UpgradeType)UnityEngine.Random.Range(0, count);
            if (!options.Contains(option)) options.Add(option);
        }
        Time.timeScale = 0f;
    }

    public bool Apply(int index)
    {
        if (!IsChoosing || index < 0 || index >= options.Count) return false;
        switch (options[index])
        {
            case GameManager.UpgradeType.MaxHealth: player.IncreaseMaxHealth(25); break;
            case GameManager.UpgradeType.Damage: player.IncreaseDamage(8); break;
            case GameManager.UpgradeType.MoveSpeed: player.IncreaseMoveSpeed(0.6f); break;
            case GameManager.UpgradeType.AttackSpeed: player.ImproveAttackSpeed(0.84f); break;
            case GameManager.UpgradeType.AttackRange: player.IncreaseAttackRange(0.22f); break;
            case GameManager.UpgradeType.Recover: player.Heal(45); break;
        }
        IsChoosing = false;
        Time.timeScale = 1f;
        return true;
    }

    public static string GetName(GameManager.UpgradeType type)
    {
        switch (type)
        {
            case GameManager.UpgradeType.MaxHealth: return "生命强化：最大生命 +25";
            case GameManager.UpgradeType.Damage: return "锋利武器：基础伤害 +8";
            case GameManager.UpgradeType.MoveSpeed: return "迅捷步伐：移速 +0.6";
            case GameManager.UpgradeType.AttackSpeed: return "战斗节奏：攻击更快";
            case GameManager.UpgradeType.AttackRange: return "长柄技巧：基础攻击范围增加";
            case GameManager.UpgradeType.Recover: return "急救补给：恢复 45 生命";
            default: return type.ToString();
        }
    }
}
