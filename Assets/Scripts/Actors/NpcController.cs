using UnityEngine;

/// <summary>NPC 预留模块：只负责身份和交互范围，暂不包含对白或剧情逻辑。</summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public sealed class NpcController : MonoBehaviour
{
    public string NpcId { get; private set; }
    public string DisplayName { get; private set; }
    public bool CanInteract { get; private set; }

    public void Configure(string npcId, string displayName)
    {
        NpcId = npcId;
        DisplayName = displayName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            CanInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            CanInteract = false;
        }
    }
}
