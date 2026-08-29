using UnityEngine;

/// <summary>使用 IMGUI 绘制中文 HUD、升级三选一和结算界面，避免额外 UI 资源依赖。</summary>
public sealed class RoguelikeHUD : MonoBehaviour
{
    private Font font;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private GUIStyle centerStyle;
    private Texture2D whiteTexture;

    private void Awake()
    {
        font = Resources.Load<Font>("Fonts/SimHei");
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    private void EnsureStyles()
    {
        if (labelStyle != null)
        {
            return;
        }
        labelStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 18, normal = { textColor = Color.white } };
        titleStyle = new GUIStyle(labelStyle) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        centerStyle = new GUIStyle(labelStyle) { fontSize = 20, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        GUI.skin.button.font = font;
    }

    private void OnGUI()
    {
        EnsureStyles();
        GameManager game = GameManager.Instance;
        PlayerController player = game.Player;
        if (player == null)
        {
            return;
        }

        DrawPanel(new Rect(18, 18, 390, 170), new Color(0.03f, 0.04f, 0.08f, 0.9f));
        GUI.Label(new Rect(34, 30, 350, 28), $"第 {game.Floor} 层　等级 {player.Level}　敌人 {game.EnemiesAlive}　击杀 {game.Kills}", labelStyle);
        DrawBar(new Rect(34, 67, 310, 22), player.Health / (float)player.MaxHealth,
            new Color(0.9f, 0.18f, 0.23f), $"生命 {player.Health}/{player.MaxHealth}");
        DrawBar(new Rect(34, 101, 310, 16), player.Experience / (float)player.ExperienceToNext,
            new Color(0.25f, 0.9f, 0.58f), $"经验 {player.Experience}/{player.ExperienceToNext}");
        GUI.Label(new Rect(34, 130, 350, 28), $"武器：{player.WeaponName}　伤害：{player.Damage}　范围：{player.AttackRange:0.00}", labelStyle);

        DrawPanel(new Rect(Screen.width - 390, 18, 372, 132), new Color(0.03f, 0.04f, 0.08f, 0.8f));
        GUI.Label(new Rect(Screen.width - 375, 30, 345, 28), "WASD 移动　J/鼠标左键 攻击", labelStyle);
        GUI.Label(new Rect(Screen.width - 375, 62, 345, 28), "F5 保存成长　Esc 返回菜单", labelStyle);
        GUI.Label(new Rect(Screen.width - 375, 94, 345, 28), $"随机种子：{game.Seed}", labelStyle);

        if (game.NearbyWeapon != null && game.NearbyWeapon.PlayerInRange)
        {
            float promptLeft = (Screen.width - 600f) * 0.5f;
            DrawPanel(new Rect(promptLeft, Screen.height - 105f, 600f, 70f), new Color(0.03f, 0.04f, 0.08f, 0.94f));
            GUI.Label(new Rect(promptLeft + 20f, Screen.height - 93f, 560f, 48f),
                $"Boss 武器：{game.NearbyWeapon.DisplayName}　[E] 替换　[Q] 保留当前武器", centerStyle);
        }

        if (!string.IsNullOrEmpty(game.StatusMessage))
        {
            GUI.Label(new Rect((Screen.width - 520f) * 0.5f, 22f, 520f, 42f), game.StatusMessage, centerStyle);
        }

        if (game.IsChoosingUpgrade)
        {
            DrawUpgradeChoice(game);
        }
        else if (game.IsGameOver)
        {
            DrawResult("冒险失败", "按 R 使用新种子重新开始");
        }
        else if (game.IsVictory)
        {
            DrawResult($"第 {game.Floor} 层已净化！", "角色成长与武器已经保留\n按 Enter 或 N 进入下一层");
        }
    }

    private void DrawUpgradeChoice(GameManager game)
    {
        float width = 760f;
        float left = (Screen.width - width) * 0.5f;
        float top = (Screen.height - 270f) * 0.5f;
        DrawPanel(new Rect(left, top, width, 270), new Color(0.025f, 0.03f, 0.07f, 0.96f));
        GUI.Label(new Rect(left, top + 22, width, 48), "升级！选择一项能力", titleStyle);
        for (int i = 0; i < game.UpgradeOptions.Count; i++)
        {
            Rect buttonRect = new Rect(left + 35f + i * 235f, top + 100f, 220f, 110f);
            string text = $"[{i + 1}]\n{GameManager.GetUpgradeName(game.UpgradeOptions[i])}";
            if (GUI.Button(buttonRect, text, new GUIStyle(GUI.skin.button) { font = font, fontSize = 18, wordWrap = true }))
            {
                game.ApplyUpgrade(i);
            }
        }
    }

    private void DrawResult(string title, string subtitle)
    {
        float left = (Screen.width - 560f) * 0.5f;
        float top = (Screen.height - 210f) * 0.5f;
        DrawPanel(new Rect(left, top, 560f, 210f), new Color(0.025f, 0.03f, 0.07f, 0.96f));
        GUI.Label(new Rect(left, top + 35, 560f, 55f), title, titleStyle);
        GUI.Label(new Rect(left + 40, top + 105, 480f, 55f), subtitle, centerStyle);
    }

    private void DrawBar(Rect rect, float ratio, Color fill, string text)
    {
        DrawPanel(rect, new Color(0.12f, 0.13f, 0.18f, 1f));
        Color old = GUI.color;
        GUI.color = fill;
        GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * Mathf.Clamp01(ratio), rect.height - 4), whiteTexture);
        GUI.color = old;
        GUI.Label(rect, text, new GUIStyle(centerStyle) { fontSize = Mathf.RoundToInt(rect.height * 0.72f) });
    }

    private void DrawPanel(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, whiteTexture);
        GUI.color = old;
    }
}
