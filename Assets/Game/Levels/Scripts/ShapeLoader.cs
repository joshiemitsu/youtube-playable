using UnityEngine;

public static class ShapeLoader
{
    public static ShapeMask FromTexture(Texture2D p_texture, float p_darkThreshold = 0.5f)
    {
        var mask = new ShapeMask(p_texture.width, p_texture.height);

        for (int x = 0; x < mask.Width; x++)
        {
            for (int y = 0; y < mask.Height; y++)
            {
                bool isDark = p_texture.GetPixel(x, y).grayscale < p_darkThreshold;
                mask.Set(x, y, isDark ? SlotType.RESTRICTED : SlotType.FREE);
            }
        }
        return mask;
    }
}
