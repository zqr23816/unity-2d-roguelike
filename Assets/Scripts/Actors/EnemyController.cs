using System.Collections.Generic;
using UnityEngine;

/// <summary>敌人 FSM；追踪状态使用地牢网格 A*，数值随层数和玩家等级成长。</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class EnemyController : MonoBehaviour
{
    private enum EnemyState { Wander, Chase, Attack, Hit, Dead }
    private readonly List<Vector2> currentPath = new List<Vector2>();
    private Rigidbody2D body;
    private PixelCharacterAnimator visualAnimator;
    private PlayerController target;
    private DungeonGenerator dungeon;
    private EnemyState state;
    private Vector2 wanderDirection;
    private float nextDecisionTime;
    private float nextAttackTime;
    private float nextPathTime;
    private float hitUntil;
    private int pathIndex;
    private int health;
    private float attackRange;
    private float chaseRange;
    private float attackCooldown;
    private bool isBoss;

    public float MoveSpeed { get; private set; }
    public int ContactDamage { get; private set; }
    public int Defense { get; private set; }
    public bool IsBoss => isBoss;

    public void Configure(PlayerController player, DungeonGenerator generator, int roomDepth,
        int floor, bool boss, GameBalanceSettings settings)
    {
        target = player;
        dungeon = generator;
        isBoss = boss;
        int floorOffset = Mathf.Max(0, floor - 1);
        int levelOffset = Mathf.Max(0, player.Level - 1);
        float healthGrowth = 1f + floorOffset * settings.enemyHealthGrowthPerFloor
            + levelOffset * settings.enemyHealthGrowthPerPlayerLevel;
        float damageGrowth = 1f + floorOffset * settings.enemyDamageGrowthPerFloor
            + levelOffset * settings.enemyDamageGrowthPerPlayerLevel;
        float bossHealth = boss ? settings.bossHealthMultiplier : 1f;
        float bossDamage = boss ? settings.bossDamageMultiplier : 1f;

        health = Mathf.RoundToInt((settings.enemyBaseHealth + roomDepth * settings.enemyHealthPerDepth) * healthGrowth * bossHealth);
        ContactDamage = Mathf.RoundToInt((settings.enemyContactDamage + roomDepth * settings.enemyDamagePerDepth) * damageGrowth * bossDamage);
        Defense = Mathf.RoundToInt(settings.enemyBaseDefense + floorOffset * settings.enemyDefensePerFloor
            + levelOffset * settings.enemyDefensePerPlayerLevel);
        MoveSpeed = settings.enemyMoveSpeed + roomDepth * settings.enemySpeedPerDepth;
        attackRange = settings.enemyAttackRange;
        chaseRange = settings.enemyChaseRange;
        attackCooldown = settings.enemyAttackCooldown;

        if (boss)
        {
            transform.localScale = Vector3.one * settings.bossScale;
            gameObject.name = "Boss - Big Zombie - Floor " + floor;
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        visualAnimator = GetComponent<PixelCharacterAnimator>();
        ChooseWanderDirection();
    }

    private void FixedUpdate()
    {
        if (state == EnemyState.Dead || target == null || GameManager.Instance.IsInputBlocked)
        {
            body.velocity = Vector2.zero;
            visualAnimator?.SetMotion(Vector2.zero);
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (state == EnemyState.Hit && Time.time < hitUntil) return;

        if (distance <= attackRange)
        {
            state = EnemyState.Attack;
            body.velocity = Vector2.zero;
            TryAttack();
        }
        else if (distance <= chaseRange)
        {
            state = EnemyState.Chase;
            FollowAStarPath();
        }
        else
        {
            state = EnemyState.Wander;
            if (Time.time >= nextDecisionTime) ChooseWanderDirection();
            body.velocity = wanderDirection * (MoveSpeed * 0.45f);
        }
        visualAnimator?.SetMotion(body.velocity);
    }

    private void FollowAStarPath()
    {
        if (dungeon == null)
        {
            body.velocity = ((Vector2)target.transform.position - body.position).normalized * MoveSpeed;
            return;
        }

        if (Time.time >= nextPathTime || pathIndex >= currentPath.Count)
        {
            dungeon.FindPath(body.position, target.transform.position, currentPath);
            pathIndex = 0;
            nextPathTime = Time.time + Random.Range(0.35f, 0.55f);
        }

        while (pathIndex < currentPath.Count && Vector2.Distance(body.position, currentPath[pathIndex]) < 0.18f) pathIndex++;
        Vector2 destination = pathIndex < currentPath.Count ? currentPath[pathIndex] : (Vector2)target.transform.position;
        body.velocity = (destination - body.position).normalized * MoveSpeed;
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;
        target.TakeDamage(ContactDamage, ((Vector2)target.transform.position - body.position).normalized);
    }

    private void ChooseWanderDirection()
    {
        wanderDirection = Random.insideUnitCircle.normalized;
        nextDecisionTime = Time.time + Random.Range(1.2f, 2.8f);
    }

    public void TakeDamage(int incomingDamage, Vector2 knockback)
    {
        if (state == EnemyState.Dead) return;
        int appliedDamage = Mathf.Max(1, incomingDamage - Defense);
        health -= appliedDamage;
        if (health <= 0) { Die(); return; }
        state = EnemyState.Hit;
        hitUntil = Time.time + 0.15f;
        visualAnimator?.TriggerHit();
        body.velocity = knockback.normalized * 7f;
    }

    private void Die()
    {
        state = EnemyState.Dead;
        body.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        GameManager.Instance.NotifyEnemyKilled(transform.position, isBoss);
        Destroy(gameObject);
    }
}
