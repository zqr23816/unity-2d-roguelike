using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>主菜单：开始新游戏、查看并读取存档、退出游戏。</summary>
public sealed class MainMenuController : MonoBehaviour
{
    private Font font;
    private Texture2D whiteTexture;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle textStyle;
    private bool showingSave;

    private void Awake()
    {
        font = Resources.Load<Font>("Fonts/SimHei");
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 44, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        buttonStyle = new GUIStyle(GUI.skin.button) { font = font, fontSize = 24 };
        textStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 18, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = new Color(0.88f, 0.92f, 1f) } };
    }

    private void OnGUI()
    {
        EnsureStyles();
        GUI.color = new Color(0.025f, 0.03f, 0.07f, 1f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTexture);
        GUI.color = Color.white;

        float panelWidth = 520f;
        float left = (Screen.width - panelWidth) * 0.5f;
        GUI.Label(new Rect(left, 95f, panelWidth, 70f), "随机地牢", titleStyle);
        GUI.Label(new Rect(left, 160f, panelWidth, 40f), "2D Roguelike", textStyle);

        if (!showingSave)
        {
            if (GUI.Button(new Rect(left + 90f, 250f, 340f, 62f), "开始游戏", buttonStyle))
            {
                RunSaveSystem.RequestNewGame();
                SceneManager.LoadScene("Main");
            }
            if (GUI.Button(new Rect(left + 90f, 330f, 340f, 62f), "储存 / 读取", buttonStyle))
            {
                showingSave = true;
            }
            if (GUI.Button(new Rect(left + 90f, 410f, 340f, 62f), "退出游戏", buttonStyle))
            {
                QuitGame();
            }
        }
        else
        {
            RunSaveData save = RunSaveSystem.Load();
            string description = save == null
                ? "暂无存档。进入游戏后按 F5 保存角色成长、武器和当前随机种子。"
                : string.Format("存档时间：{0}\n层数：{1}　等级：{2}　生命：{3}/{4}　武器：{5}", save.savedAt, Mathf.Max(1, save.floor), save.level, save.health, save.maxHealth, GetWeaponName(save.weapon));
            GUI.Label(new Rect(left + 30f, 235f, panelWidth - 60f, 100f), description, textStyle);
            GUI.enabled = save != null;
            if (GUI.Button(new Rect(left + 90f, 350f, 340f, 62f), "读取存档", buttonStyle))
            {
                RunSaveSystem.RequestLoad();
                SceneManager.LoadScene("Main");
            }
            GUI.enabled = true;
            if (GUI.Button(new Rect(left + 90f, 430f, 340f, 62f), "返回", buttonStyle))
            {
                showingSave = false;
            }
        }
    }

    private static string GetWeaponName(WeaponId id)
    {
        switch (id)
        {
            case WeaponId.SteelSword: return "钢剑";
            case WeaponId.GoldenSword: return "黄金剑";
            case WeaponId.Hammer: return "战锤";
            case WeaponId.SilverKatana: return "银色武士刀";
            default: return "劈刀";
        }
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
