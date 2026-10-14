using System.Collections.Generic;
using RapidPro.GameLoop;
using UnityEngine;

/// <summary>
/// A saved arrow level: board size, the cells outside the shape, and every arrow.
/// Made by the level generator (YT-Playables > Levels menu); the game only reads it.
/// </summary>
[CreateAssetMenu(fileName = "ArrowLevel", menuName = "YT-Playables/Arrow Level")]
public class ArrowLevelData : LevelData
{
    [Header("Board")]
    [SerializeField, Min(1)] private int _width = 11;
    [SerializeField, Min(1)] private int _height = 11;

    [Tooltip("Cells outside the shape. Arrows leave the board when they reach one.")]
    [SerializeField] private List<Vector2Int> _restrictedCells = new List<Vector2Int>();

    [Header("Arrows")]
    [SerializeField] private List<ArrowDefinition> _arrows = new List<ArrowDefinition>();

    [Header("Info")]
    [Tooltip("Seed the generator used, so the level can be made again.")]
    [SerializeField] private int _seed;

    public int Width => _width;
    public int Height => _height;
    public IReadOnlyList<Vector2Int> RestrictedCells => _restrictedCells;
    public IReadOnlyList<ArrowDefinition> Arrows => _arrows;
    public int Seed => _seed;

    public bool IsPlayable(Vector2Int p_cell)
    {
        bool isInBounds = p_cell.x >= 0 && p_cell.y >= 0 && p_cell.x < _width && p_cell.y < _height;
        return isInBounds && !_restrictedCells.Contains(p_cell);
    }

    /// <summary>Overwrites the level. Used by the level generator.</summary>
    public void Set(int p_width, int p_height, List<Vector2Int> p_restrictedCells, List<ArrowDefinition> p_arrows, int p_seed)
    {
        _width = p_width;
        _height = p_height;
        _restrictedCells = p_restrictedCells;
        _arrows = p_arrows;
        _seed = p_seed;
    }
}
