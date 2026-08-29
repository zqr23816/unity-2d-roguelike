using UnityEngine;

/// <summary>自动吸附到玩家的经验掉落物。</summary>
[RequireComponent(typeof(CircleCollider2D), typeof(SpriteRenderer))]
public sealed class ExperienceOrb : MonoBehaviour
{
    private PlayerController target;
    private int value;

    public void Configure(PlayerController player, int experienceValue)
    {
        target = player;
        value = experienceValue;
    }

    private void Awake()
    {
        CircleCollider2D trigger = GetComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.45f;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.Square;
        renderer.color = new Color(0.3f, 1f, 0.62f);
        renderer.sortingOrder = 2;
        transform.localScale = Vector3.one * 0.24f;
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance < 4f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.transform.position,
                (3f + (4f - distance) * 2f) * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        player.GainExperience(value);
        Destroy(gameObject);
    }
}
