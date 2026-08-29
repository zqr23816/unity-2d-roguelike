using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>生成数值配置、菜单场景、主场景与 Windows 测试包。</summary>
public static class RoguelikeProjectBuilder
{
    private const string MenuPath = "Assets/Scenes/Menu.unity";
    private const string MainPath = "Assets/Scenes/Main.unity";
    private const string SettingsPath = "Assets/Resources/Settings/GameBalanceSettings.asset";

    [MenuItem("Roguelike/生成菜单与主场景")]
    public static void CreateAllScenes()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Resources/Settings");
        GameBalanceSettings settings = CreateOrLoadSettings();
        CreateMenuScene();
        CreateMainScene(settings);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuPath, true),
            new EditorBuildSettingsScene(MainPath, true)
        };
        ConfigurePlayerSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("菜单、数值配置和主场景生成完成。可在 Assets/Resources/Settings 中调参。");
    }

    private static GameBalanceSettings CreateOrLoadSettings()
    {
        GameBalanceSettings settings = AssetDatabase.LoadAssetAtPath<GameBalanceSettings>(SettingsPath);
        if (settings != null) return settings;
        settings = ScriptableObject.CreateInstance<GameBalanceSettings>();
        settings.ResetToRecommendedDefaults();
        AssetDatabase.CreateAsset(settings, SettingsPath);
        EditorUtility.SetDirty(settings);
        return settings;
    }

    private static void CreateMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraObject = new GameObject("Menu Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.03f, 0.07f);
        cameraObject.AddComponent<AudioListener>();
        new GameObject("Main Menu").AddComponent<MainMenuController>();
        EditorSceneManager.SaveScene(scene, MenuPath);
    }

    private static void CreateMainScene(GameBalanceSettings settings)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.backgroundColor = new Color(0.025f, 0.03f, 0.06f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraFollow>();
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        GameManager manager = new GameObject("Game Manager").AddComponent<GameManager>();
        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("balanceSettings").objectReferenceValue = settings;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene, MainPath);
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = "PlanKr Portfolio";
        PlayerSettings.productName = "随机地牢：Roguelike 2D";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
    }

    [MenuItem("Roguelike/构建 Windows 版本")]
    public static void BuildWindows()
    {
        CreateAllScenes();
        Directory.CreateDirectory("Build");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { MenuPath, MainPath },
            locationPathName = "Build/Roguelike2D.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        string result = $"构建结果：{report.summary.result}\n总大小：{report.summary.totalSize}\n耗时：{report.summary.totalTime}\n";
        File.WriteAllText("BuildReport.txt", result);
        if (report.summary.result != BuildResult.Succeeded) throw new System.Exception(result);
        Debug.Log(result);
    }
}
