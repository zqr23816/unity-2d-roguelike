using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>管理随机种子、楼层推进和存档请求，不参与战斗与实体生成。</summary>
public sealed class RunSession
{
    public int Seed { get; private set; }
    public int Floor { get; private set; } = 1;
    public RunSaveData InitialSave { get; private set; }

    public void Initialize()
    {
        InitialSave = RunSaveSystem.ConsumeLoadRequest();
        Floor = InitialSave != null ? Mathf.Max(1, InitialSave.floor) : 1;
        Seed = InitialSave != null && InitialSave.seed != 0
            ? InitialSave.seed
            : unchecked((int)DateTime.Now.Ticks);
        UnityEngine.Random.InitState(Seed);
    }

    public void Save(PlayerController player)
    {
        if (player != null) RunSaveSystem.Save(player.CreateSaveData(Seed, Floor));
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        RunSaveSystem.RequestNewGame();
        SceneManager.LoadScene("Main");
    }

    public void Advance(PlayerController player)
    {
        Time.timeScale = 1f;
        RunSaveData nextFloor = player.CreateSaveData(0, Floor + 1);
        RunSaveSystem.Save(nextFloor);
        RunSaveSystem.RequestNextFloor(nextFloor);
        SceneManager.LoadScene("Main");
    }

    public static void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
