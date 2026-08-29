using UnityEngine;

/// <summary>玩家移动、近战攻击、生命值、武器与 Roguelike 成长属性。</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class PlayerController : MonoBehaviour
{
    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private PixelCharacterAnimator visualAnimator;
    private WeaponController weaponController;
    private Vector2 movement;
    private Vector2 facing = Vector2.right;
    private float nextAttackTime;
    private float invulnerableUntil;
    private float invulnerabilityDuration = 0.65f;

    public float MoveSpeed { get; private set; } = 5f;
    public int MaxHealth { get; private set; } = 100;
    public int Health { get; private set; } = 100;
    public int BaseDamage { get; private set; } = 20;
    public float BaseAttackRange { get; private set; } = 1.25f;
    public float BaseAttackCooldown { get; private set; } = 0.35f;
    public int Damage => BaseDamage + (weaponController != null ? weaponController.DamageBonus : 0);
    public float AttackRange => BaseAttackRange * (weaponController != null ? weaponController.RangeMultiplier : 1f);
    public float AttackCooldown => BaseAttackCooldown * (weaponController != null ? weaponController.CooldownMultiplier : 1f);
    public string WeaponName => weaponController != null ? weaponController.DisplayName : "劈刀";
    public WeaponId CurrentWeapon => weaponController != null ? weaponController.CurrentWeapon : WeaponId.Chopper;
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; }
    public int ExperienceToNext => 8 + Level * 4;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        spriteRenderer = GetComponent<SpriteRenderer>();
        visualAnimator = GetComponent<PixelCharacterAnimator>();
        weaponController = GetComponent<WeaponController>();
    }

    public void Configure(GameBalanceSettings settings)
    {
        MaxHealth = settings.playerMaxHealth;
        Health = MaxHealth;
        MoveSpeed = settings.playerMoveSpeed;
        BaseDamage = settings.playerBaseDamage;
        BaseAttackRange = settings.playerAttackRange;
        BaseAttackCooldown = settings.playerAttackCooldown;
        invulnerabilityDuration = settings.playerInvulnerability;
        if (weaponController == null)
        {
            Debug.LogError("Player Prefab 缺少 WeaponController，已使用兼容兜底补充组件。");
            weaponController = gameObject.AddComponent<WeaponController>();
        }
        weaponController.Initialize(settings, settings.initialWeapon);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsInputBlocked)
        {
            movement = Vector2.zero;
            visualAnimator?.SetMotion(movement);
            return;
        }

        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (movement.sqrMagnitude > 0.01f)
        {
            facing = movement;
            weaponController?.SetFacing(facing);
        }
        visualAnimator?.SetMotion(movement);

        if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttackTime)
        {
            Attack();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Time.time < invulnerableUntil && Mathf.FloorToInt(Time.time * 16f) % 2 == 0
                ? new Color(1f, 1f, 1f, 0.35f)
                : Color.white;
        }
    }

    private void FixedUpdate() { body.velocity = movement * MoveSpeed; }

    private void Attack()
    {
        nextAttackTime = Time.time + AttackCooldown;
        weaponController?.PerformAttack(Damage, facing, AttackRange);
    }

    public void EquipWeapon(WeaponId weapon) { weaponController?.Equip(weapon); }

    public void TakeDamage(int amount, Vector2 knockback)
    {
        if (Time.time < invulnerableUntil || Health <= 0) return;
        Health = Mathf.Max(0, Health - Mathf.Max(1, amount));
        invulnerableUntil = Time.time + invulnerabilityDuration;
        visualAnimator?.TriggerHit();
        body.AddForce(knockback.normalized * 5f, ForceMode2D.Impulse);
        if (Health == 0)
        {
            body.velocity = Vector2.zero;
            GameManager.Instance.NotifyPlayerDied();
        }
    }

    public void GainExperience(int amount)
    {
        Experience += Mathf.Max(0, amount);
        if (Experience >= ExperienceToNext)
        {
            Experience -= ExperienceToNext;
            Level++;
            GameManager.Instance.BeginUpgradeChoice();
        }
    }

    public void Heal(int amount) { Health = Mathf.Min(MaxHealth, Health + Mathf.Max(0, amount)); }
    public void IncreaseMaxHealth(int amount) { MaxHealth += amount; Health += amount; }
    public void IncreaseDamage(int amount) { BaseDamage += amount; }
    public void IncreaseMoveSpeed(float amount) { MoveSpeed += amount; }
    public void ImproveAttackSpeed(float multiplier) { BaseAttackCooldown = Mathf.Max(0.08f, BaseAttackCooldown * multiplier); }
    public void IncreaseAttackRange(float amount) { BaseAttackRange += amount; }

    public RunSaveData CreateSaveData(int seed, int floor)
    {
        return new RunSaveData
        {
            seed = seed, floor = floor, maxHealth = MaxHealth, health = Health, moveSpeed = MoveSpeed,
            baseDamage = BaseDamage, baseAttackRange = BaseAttackRange, baseAttackCooldown = BaseAttackCooldown,
            level = Level, experience = Experience, weapon = CurrentWeapon
        };
    }

    public void Restore(RunSaveData data)
    {
        MaxHealth = Mathf.Max(1, data.maxHealth);
        Health = Mathf.Clamp(data.health, 1, MaxHealth);
        MoveSpeed = Mathf.Max(0.1f, data.moveSpeed);
        BaseDamage = Mathf.Max(1, data.baseDamage);
        BaseAttackRange = Mathf.Max(0.1f, data.baseAttackRange);
        BaseAttackCooldown = Mathf.Max(0.05f, data.baseAttackCooldown);
        Level = Mathf.Max(1, data.level);
        Experience = Mathf.Max(0, data.experience);
        EquipWeapon(data.weapon);
    }
}
