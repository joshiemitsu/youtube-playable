using UnityEngine;

public static class ShapeResizer
{
    public static ShapeMask Resize(ShapeMask p_source, int p_width, int p_height)
    {
        var result = new ShapeMask(p_width, p_height);

        for (int x = 0; x < p_width; x++)
        {
            for (int y = 0; y < p_height; y++)
            {
                int sourceX = x * p_source.Width / p_width;
                int sourceY = y * p_source.Height / p_height;
                result.Set(x, y, p_source.Get(sourceX, sourceY));
            }
        }
        return result;
    }
}
