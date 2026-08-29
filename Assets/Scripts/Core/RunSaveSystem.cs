using System;
using System.IO;
using UnityEngine;

[Serializable]
public sealed class RunSaveData
{
    public int floor = 1;
    public int seed;
    public int maxHealth;
    public int health;
    public float moveSpeed;
    public int baseDamage;
    public float baseAttackRange;
    public float baseAttackCooldown;
    public int level;
    public int experience;
    public WeaponId weapon;
    public string savedAt;
}

/// <summary>保存角色成长与随机种子；读取后使用同一种子重建地牢。</summary>
public static class RunSaveSystem
{
    private static RunSaveData pendingRun;
    private static string SavePath => Path.Combine(Application.persistentDataPath, "run-save.json");

    public static bool HasSave => File.Exists(SavePath);

    public static void Save(RunSaveData data)
    {
        data.savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public static RunSaveData Load()
    {
        if (!HasSave)
        {
            return null;
        }
        return JsonUtility.FromJson<RunSaveData>(File.ReadAllText(SavePath));
    }

    public static void RequestLoad()
    {
        pendingRun = Load();
    }

    public static void RequestNewGame()
    {
        pendingRun = null;
    }

    public static void RequestNextFloor(RunSaveData data)
    {
        pendingRun = data;
    }

    public static RunSaveData ConsumeLoadRequest()
    {
        RunSaveData result = pendingRun;
        pendingRun = null;
        return result;
    }
}
