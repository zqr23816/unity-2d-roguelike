using UnityEngine;

/// <summary>运行时生成纯白像素 Sprite，各对象通过 SpriteRenderer.color 着色。</summary>
public static class RuntimeSpriteFactory
{
    private static Sprite square;

    public static Sprite Square
    {
        get
        {
            if (square != null)
            {
                return square;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "Runtime White Pixel",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            square.name = "Runtime Square";
            return square;
        }
    }

    public static SpriteRenderer AddRenderer(GameObject owner, Color color, int order, Vector2 size)
    {
        SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
        renderer.sprite = Square;
        renderer.color = color;
        renderer.sortingOrder = order;
        owner.transform.localScale = new Vector3(size.x, size.y, 1f);
        return renderer;
    }
}
