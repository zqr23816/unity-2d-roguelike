using System.Collections.Generic;
using UnityEngine;

/// <summary>由 BoxCollider2D 提供的近战判定框，只在挥砍窗口内启用。</summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class MeleeAttackBox : MonoBehaviour
{
    private readonly HashSet<EnemyController> hitTargets = new HashSet<EnemyController>();
    private readonly Collider2D[] overlapBuffer = new Collider2D[32];
    private BoxCollider2D attackCollider;
    private int damage;
    private Vector2 knockbackDirection;
    private float activeUntil;

    public BoxCollider2D Collider => attackCollider;

    private void Awake()
    {
        attackCollider = GetComponent<BoxCollider2D>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;
    }

    public void PerformAttack(int attackDamage, Vector2 direction, float range, float duration)
    {
        damage = Mathf.Max(1, attackDamage);
        knockbackDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.right;
        hitTargets.Clear();

        float boxLength = Mathf.Max(0.35f, range);
        attackCollider.size = new Vector2(boxLength, 0.62f);
        attackCollider.offset = new Vector2(boxLength * 0.5f, 0f);
        attackCollider.enabled = true;
        activeUntil = Time.time + duration;
        DetectCurrentOverlaps();
    }

    private void Update()
    {
        if (attackCollider.enabled && Time.time >= activeUntil)
        {
            attackCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { TryHit(other); }
    private void OnTriggerStay2D(Collider2D other) { TryHit(other); }

    private void DetectCurrentOverlaps()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        int count = attackCollider.OverlapCollider(filter, overlapBuffer);
        for (int i = 0; i < count; i++) TryHit(overlapBuffer[i]);
    }

    private void TryHit(Collider2D other)
    {
        if (!attackCollider.enabled) return;
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null && hitTargets.Add(enemy)) enemy.TakeDamage(damage, knockbackDirection);
    }
}
