using UnityEngine;

/// <summary>轻量级逐帧动画播放器，用代码切换待机、移动与受击帧。</summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PixelCharacterAnimator : MonoBehaviour
{
    public enum CharacterKind
    {
        KnightFemale,
        Goblin,
        OrcWarrior,
        BigZombie
    }

    private SpriteRenderer spriteRenderer;
    private Sprite[] idleFrames = new Sprite[0];
    private Sprite[] runFrames = new Sprite[0];
    private Sprite hitFrame;
    private bool moving;
    private float hitUntil;
    private float framesPerSecond = 8f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Configure(CharacterKind kind)
    {
        string root;
        string idle;
        string run;
        string hit = null;

        switch (kind)
        {
            case CharacterKind.KnightFemale:
                root = "Art/0x72/DungeonTilesetII/Player";
                idle = "knight_f_idle_anim";
                run = "knight_f_run_anim";
                hit = "knight_f_hit_anim_f0";
                framesPerSecond = 9f;
                break;
            case CharacterKind.OrcWarrior:
                root = "Art/0x72/DungeonTilesetII/Enemies/OrcWarrior";
                idle = "orc_warrior_idle_anim";
                run = "orc_warrior_run_anim";
                break;
            case CharacterKind.BigZombie:
                root = "Art/0x72/DungeonTilesetII/Enemies/BigZombie";
                idle = "big_zombie_idle_anim";
                run = "big_zombie_run_anim";
                framesPerSecond = 7f;
                break;
            default:
                root = "Art/0x72/DungeonTilesetII/Enemies/Goblin";
                idle = "goblin_idle_anim";
                run = "goblin_run_anim";
                break;
        }

        idleFrames = LoadSequence(root + "/" + idle, 4);
        runFrames = LoadSequence(root + "/" + run, 4);
        hitFrame = string.IsNullOrEmpty(hit) ? null : Resources.Load<Sprite>(root + "/" + hit);
        if (idleFrames.Length == 0)
        {
            Debug.LogError("角色待机动画加载失败：" + root + "/" + idle, this);
        }
        if (idleFrames.Length > 0)
        {
            spriteRenderer.sprite = idleFrames[0];
        }
    }

    public void SetMotion(Vector2 velocity)
    {
        moving = velocity.sqrMagnitude > 0.01f;
        if (Mathf.Abs(velocity.x) > 0.01f)
        {
            spriteRenderer.flipX = velocity.x < 0f;
        }
    }

    public void TriggerHit(float duration = 0.14f)
    {
        hitUntil = Time.time + duration;
    }

    private void Update()
    {
        if (hitFrame != null && Time.time < hitUntil)
        {
            spriteRenderer.sprite = hitFrame;
            return;
        }

        Sprite[] frames = moving && runFrames.Length > 0 ? runFrames : idleFrames;
        if (frames.Length == 0)
        {
            return;
        }

        int frame = Mathf.FloorToInt(Time.time * framesPerSecond) % frames.Length;
        spriteRenderer.sprite = frames[frame];
    }

    private static Sprite[] LoadSequence(string prefix, int count)
    {
        Sprite[] buffer = new Sprite[count];
        int loaded = 0;
        for (int i = 0; i < count; i++)
        {
            Sprite sprite = Resources.Load<Sprite>(prefix + "_f" + i);
            if (sprite != null)
            {
                buffer[loaded++] = sprite;
            }
        }

        if (loaded == count)
        {
            return buffer;
        }

        Sprite[] result = new Sprite[loaded];
        for (int i = 0; i < loaded; i++)
        {
            result[i] = buffer[i];
        }
        return result;
    }
}
