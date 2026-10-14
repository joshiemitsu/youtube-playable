using UnityEngine;

public class ShapeMask
{
    public int Width;
    public int Height;
    public SlotType[] Cells;

    public ShapeMask(int p_width, int p_height)
    {
        Width = p_width;
        Height = p_height;
        Cells = new SlotType[p_width * p_height];
    }

    public bool IsInBounds(int p_x, int p_y) => p_x >= 0 && p_y >= 0 && p_x < Width && p_y < Height;
    public SlotType Get(int p_x, int p_y) => Cells[p_x + p_y * Width];
    public void Set(int p_x, int p_y, SlotType p_value) => Cells[p_x + p_y * Width] = p_value;
}
