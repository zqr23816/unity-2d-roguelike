using UnityEngine;

public enum WeaponId { Chopper, SteelSword, GoldenSword, Hammer, SilverKatana }

/// <summary>管理当前武器配置，并把武器素材与攻击判定交给 WeaponSocket。</summary>
public sealed class WeaponController : MonoBehaviour
{
    private GameBalanceSettings settings;
    private GameBalanceSettings.WeaponTuning tuning;
    private WeaponSocket socket;

    public WeaponId CurrentWeapon => tuning != null ? tuning.id : WeaponId.Chopper;
    public string DisplayName => tuning != null ? tuning.displayName : "劈刀";
    public int DamageBonus => tuning != null ? tuning.damageBonus : 0;
    public float RangeMultiplier => tuning != null ? tuning.rangeMultiplier : 1f;
    public float CooldownMultiplier => tuning != null ? tuning.cooldownMultiplier : 1f;
    public WeaponSocket Socket => socket;

    public void Initialize(GameBalanceSettings gameSettings, WeaponId initialWeapon)
    {
        settings = gameSettings;
        socket = GetComponentInChildren<WeaponSocket>(true);
        if (socket == null)
        {
            Debug.LogWarning("Player Prefab 缺少 Hand Point，已使用兼容兜底创建。请重新运行项目生成器。", this);
            GameObject hand = new GameObject("Hand Point");
            hand.transform.SetParent(transform, false);
            BoxCollider2D box = hand.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            MeleeAttackBox attackBox = hand.AddComponent<MeleeAttackBox>();
            GameObject visual = new GameObject("Equipped Weapon");
            visual.transform.SetParent(hand.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            socket = hand.AddComponent<WeaponSocket>();
            socket.Initialize(renderer, visual.transform, attackBox);
        }
        else
        {
            MeleeAttackBox attackBox = socket.GetComponent<MeleeAttackBox>();
            SpriteRenderer renderer = socket.GetComponentInChildren<SpriteRenderer>(true);
            socket.Initialize(renderer, renderer != null ? renderer.transform : socket.transform, attackBox);
        }
        Equip(initialWeapon);
    }

    public void Equip(WeaponId weapon)
    {
        GameBalanceSettings.WeaponTuning next = settings.GetWeapon(weapon);
        if (next == null) { Debug.LogWarning("没有找到武器配置：" + weapon); return; }
        Sprite sprite = Resources.Load<Sprite>(next.resourcePath);
        if (sprite == null) { Debug.LogError("武器贴图加载失败：" + next.resourcePath); return; }
        tuning = next;
        socket.BindWeapon(sprite);
    }

    public void SetFacing(Vector2 direction)
    {
        socket?.SetFacing(direction);
    }

    public void PerformAttack(int damage, Vector2 direction, float range)
    {
        if (socket == null) return;
        socket.SetFacing(direction);
        socket.Swing(damage, range);
    }
}
