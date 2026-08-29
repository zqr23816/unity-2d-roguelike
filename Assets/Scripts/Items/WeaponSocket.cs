using UnityEngine;

/// <summary>角色手部挂点组件；负责武器素材绑定、方向、遮挡顺序和挥砍旋转。</summary>
public sealed class WeaponSocket : MonoBehaviour
{
    [SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private MeleeAttackBox attackBox;
    private Transform weaponVisual;
    private Vector2 facing = Vector2.right;
    private float swingStartedAt;
    private float swingUntil;

    public void Initialize(SpriteRenderer renderer, Transform visual, MeleeAttackBox meleeBox)
    {
        weaponRenderer = renderer;
        weaponVisual = visual;
        attackBox = meleeBox;
        weaponVisual.localPosition = Vector3.right * 0.34f;
        weaponVisual.localRotation = Quaternion.Euler(0f, 0f, -45f);
        weaponVisual.localScale = Vector3.one * 0.82f;
        SetFacing(Vector2.right);
    }

    public void BindWeapon(Sprite sprite)
    {
        weaponRenderer.sprite = sprite;
    }

    public void SetFacing(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.01f) return;
        facing = direction.normalized;
        UpdateHandPosition();
    }

    public void Swing(int damage, float range)
    {
        swingStartedAt = Time.time;
        swingUntil = Time.time + 0.18f;
        attackBox.PerformAttack(damage, facing, range, 0.18f);
    }

    private void LateUpdate()
    {
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        if (Time.time < swingUntil)
        {
            float progress = Mathf.InverseLerp(swingStartedAt, swingUntil, Time.time);
            float swingDirection = facing.x < 0f ? 1f : -1f;
            angle += Mathf.Lerp(-55f, 75f, Mathf.SmoothStep(0f, 1f, progress)) * swingDirection;
        }
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdateHandPosition()
    {
        if (Mathf.Abs(facing.x) >= Mathf.Abs(facing.y))
            transform.localPosition = new Vector3(facing.x < 0f ? -0.24f : 0.24f, 0.02f, 0f);
        else if (facing.y > 0f)
            transform.localPosition = new Vector3(0.08f, 0.2f, 0f);
        else
            transform.localPosition = new Vector3(0.1f, -0.08f, 0f);

        weaponRenderer.flipY = facing.x < -0.01f;
        weaponRenderer.sortingOrder = facing.y > 0.2f ? 2 : 4;
    }
}
