using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>生成数值配置、实体 Prefab、Prefab 目录、场景与 Windows 测试包。</summary>
public static class RoguelikeProjectBuilder
{
    private const string MenuPath = "Assets/Scenes/Menu.unity";
    private const string MainPath = "Assets/Scenes/Main.unity";
    private const string SettingsPath = "Assets/Resources/Settings/GameBalanceSettings.asset";
    private const string CatalogPath = "Assets/Resources/Settings/RoguelikePrefabCatalog.asset";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    private const string EnemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
    private const string ExperienceOrbPrefabPath = "Assets/Prefabs/ExperienceOrb.prefab";
    private const string WeaponPickupPrefabPath = "Assets/Prefabs/WeaponPickup.prefab";

    [MenuItem("Roguelike/生成菜单、Prefab 与主场景")]
    public static void CreateAllScenes()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Resources/Settings");
        Directory.CreateDirectory("Assets/Prefabs");
        GameBalanceSettings settings = CreateOrLoadSettings();
        RoguelikePrefabCatalog catalog = CreateGameplayPrefabs();
        CreateMenuScene();
        CreateMainScene(settings, catalog);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuPath, true),
            new EditorBuildSettingsScene(MainPath, true)
        };
        ConfigurePlayerSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("菜单、实体 Prefab、Prefab 目录、数值配置和主场景生成完成。");
    }

    private static RoguelikePrefabCatalog CreateGameplayPrefabs()
    {
        GameObject playerPrefab = CreatePlayerPrefab();
        GameObject enemyPrefab = CreateEnemyPrefab();
        GameObject orbPrefab = CreateExperienceOrbPrefab();
        GameObject weaponPrefab = CreateWeaponPickupPrefab();

        RoguelikePrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<RoguelikePrefabCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<RoguelikePrefabCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }
        catalog.Configure(playerPrefab, enemyPrefab, orbPrefab, weaponPrefab);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static GameObject CreatePlayerPrefab()
    {
        GameObject owner = new GameObject("Player");
        SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 3;
        owner.AddComponent<PixelCharacterAnimator>();
        Rigidbody2D body = owner.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        CircleCollider2D collider = owner.AddComponent<CircleCollider2D>();
        collider.radius = 0.48f;
        owner.AddComponent<PlayerController>();
        owner.AddComponent<WeaponController>();

        GameObject hand = new GameObject("Hand Point");
        hand.transform.SetParent(owner.transform, false);
        BoxCollider2D attackCollider = hand.AddComponent<BoxCollider2D>();
        attackCollider.isTrigger = true;
        hand.AddComponent<MeleeAttackBox>();
        hand.AddComponent<WeaponSocket>();
        GameObject weapon = new GameObject("Equipped Weapon");
        weapon.transform.SetParent(hand.transform, false);
        weapon.AddComponent<SpriteRenderer>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(owner, PlayerPrefabPath);
        Object.DestroyImmediate(owner);
        return prefab;
    }

    private static GameObject CreateEnemyPrefab()
    {
        GameObject owner = new GameObject("Enemy");
        SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 2;
        owner.AddComponent<PixelCharacterAnimator>();
        Rigidbody2D body = owner.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        CircleCollider2D collider = owner.AddComponent<CircleCollider2D>();
        collider.radius = 0.47f;
        owner.AddComponent<EnemyController>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(owner, EnemyPrefabPath);
        Object.DestroyImmediate(owner);
        return prefab;
    }

    private static GameObject CreateExperienceOrbPrefab()
    {
        GameObject owner = new GameObject("Experience Orb");
        owner.AddComponent<SpriteRenderer>().sortingOrder = 2;
        CircleCollider2D collider = owner.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.45f;
        owner.AddComponent<ExperienceOrb>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(owner, ExperienceOrbPrefabPath);
        Object.DestroyImmediate(owner);
        return prefab;
    }

    private static GameObject CreateWeaponPickupPrefab()
    {
        GameObject owner = new GameObject("Weapon Pickup");
        owner.AddComponent<SpriteRenderer>().sortingOrder = 5;
        CircleCollider2D collider = owner.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.8f;
        owner.AddComponent<WeaponPickup>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(owner, WeaponPickupPrefabPath);
        Object.DestroyImmediate(owner);
        return prefab;
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

    private static void CreateMainScene(GameBalanceSettings settings, RoguelikePrefabCatalog catalog)
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
        serializedManager.FindProperty("prefabCatalog").objectReferenceValue = catalog;
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
