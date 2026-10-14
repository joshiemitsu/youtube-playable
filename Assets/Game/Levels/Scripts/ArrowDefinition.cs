using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One arrow in a level: the cells it covers (head first, then body to tail) and the way its head points.
/// </summary>
[Serializable]
public class ArrowDefinition
{
    [Tooltip("Board cells from the head to the tail. Each cell touches the one before it.")]
    [SerializeField] private List<Vector2Int> _cells = new List<Vector2Int>();

    [Tooltip("Direction the head points: up, down, left or right.")]
    [SerializeField] private Vector2Int _direction = Vector2Int.up;

    public ArrowDefinition(List<Vector2Int> p_cells, Vector2Int p_direction)
    {
        _cells = p_cells;
        _direction = p_direction;
    }

    public IReadOnlyList<Vector2Int> Cells => _cells;
    public Vector2Int Direction => _direction;
    public Vector2Int Head => _cells[0];
}
