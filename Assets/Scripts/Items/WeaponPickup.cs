using UnityEngine;

/// <summary>Boss 武器掉落；玩家进入范围后可选择替换或保留当前武器。</summary>
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
public sealed class WeaponPickup : MonoBehaviour
{
    private WeaponId weaponId;
    private PlayerController player;
    private bool playerInRange;
    private bool resolved;

    public WeaponId WeaponId => weaponId;
    public bool PlayerInRange => playerInRange;
    public string DisplayName { get; private set; }

    public void Configure(WeaponId id, GameBalanceSettings settings)
    {
        weaponId = id;
        GameBalanceSettings.WeaponTuning tuning = settings.GetWeapon(id);
        DisplayName = tuning != null ? tuning.displayName : id.ToString();
        GetComponent<SpriteRenderer>().sprite = tuning != null ? Resources.Load<Sprite>(tuning.resourcePath) : null;
        GetComponent<SpriteRenderer>().sortingOrder = 5;
        CircleCollider2D trigger = GetComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.8f;
    }

    private void Update()
    {
        if (!playerInRange || resolved || player == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            resolved = true;
            player.EquipWeapon(weaponId);
            GameManager.Instance.ResolveBossReward(true, DisplayName);
            Destroy(gameObject);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            resolved = true;
            GameManager.Instance.ResolveBossReward(false, DisplayName);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController candidate = other.GetComponent<PlayerController>();
        if (candidate == null)
        {
            return;
        }

        player = candidate;
        playerInRange = true;
        GameManager.Instance.SetNearbyWeapon(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == player)
        {
            playerInRange = false;
            GameManager.Instance.SetNearbyWeapon(null);
        }
    }
}
