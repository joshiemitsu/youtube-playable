using System.Collections.Generic;
using RapidPro.Core;
using RapidPro.Pooling;
using UnityEngine;

// Builds a level on the board: dots for every playable cell and the arrows, all from the pool
public class BoardInitializer : MonoBehaviour
{
    [Header("Main System")]
    [SerializeField] private GameBoardSystem _boardSystem;

    [Header("Prefab")]
    [SerializeField] private GameObject _dotsPrefab;
    [SerializeField] private GameObject _arrowPrefab;

    [Header("Look")]
    [Tooltip("Arrow colours, used in turn so neighbouring arrows are easy to tell apart.")]
    [SerializeField] private Color[] _arrowColors =
    {
        new Color(0.32f, 0.91f, 0.06f),
        new Color(0.13f, 0.59f, 0.95f),
        new Color(1.00f, 0.60f, 0.00f),
        new Color(0.91f, 0.12f, 0.39f),
        new Color(0.61f, 0.15f, 0.69f),
        new Color(0.00f, 0.74f, 0.83f),
    };

    private readonly List<GameObject> _dots = new List<GameObject>();
    private readonly List<ArrowEntry> _arrows = new List<ArrowEntry>();
    private IPoolService _pool;

    // Arrows still on the board
    public IReadOnlyList<ArrowEntry> Arrows => _arrows;

    public void Build(ArrowLevelData p_level, IPoolService p_pool)
    {
        _pool = p_pool;
        _boardSystem.Setup(p_level.Width, p_level.Height, p_level.RestrictedCells);

        SpawnDots(p_level);
        SpawnArrows(p_level);
    }

    // Removes an arrow that has left the board
    public void Despawn(ArrowEntry p_arrow)
    {
        p_arrow.DespawnParts();
        _pool.Despawn(p_arrow.gameObject);
        _arrows.Remove(p_arrow);
    }

    // Returns everything on the board to the pool
    public void Clear()
    {
        for (int i = _arrows.Count - 1; i >= 0; i--)
        {
            Despawn(_arrows[i]);
        }

        foreach (var dot in _dots)
        {
            _pool.Despawn(dot);
        }
        _dots.Clear();
    }

    private void SpawnDots(ArrowLevelData p_level)
    {
        for (int w = 0; w < p_level.Width; w++)
        {
            for (int h = 0; h < p_level.Height; h++)
            {
                Vector2 cell = new Vector2(w, h);
                if (!_boardSystem.IsPlayable(cell)) continue;

                GameObject dot = _pool.Spawn(_dotsPrefab, _boardSystem.GetWorldPosition(cell), Quaternion.identity);
                dot.transform.SetParent(_boardSystem.WorldParent, true);
                _dots.Add(dot);
            }
        }
    }

    private void SpawnArrows(ArrowLevelData p_level)
    {
        for (int i = 0; i < p_level.Arrows.Count; i++)
        {
            ArrowDefinition definition = p_level.Arrows[i];

            List<Vector2> points = new List<Vector2>();
            foreach (Vector2Int cell in definition.Cells)
            {
                Vector2 point = new Vector2(cell.x, cell.y);
                points.Add(point);
                _boardSystem.SetSlot(point, (int)SlotType.OCCUPIED);
            }

            ArrowData data = new ArrowData();
            data.Points = points;
            data.Direction = new Vector2(definition.Direction.x, definition.Direction.y);

            // The head is drawn facing away from its second segment, so the arrow must also move that way
            bool facesItsMove = points.Count < 2 || points[0] - points[1] == data.Direction;
            if (!facesItsMove)
            {
                Log.Warning($"Arrow {i} in {p_level.name} moves {data.Direction} but its head faces {points[0] - points[1]}. Regenerate the level.");
            }
            data.StartPoint = points[0]; // Get first index as the start position

            GameObject obj = _pool.Spawn(_arrowPrefab, _boardSystem.GetWorldPosition(points[0]), Quaternion.identity);
            obj.transform.SetParent(_boardSystem.WorldParent, true);

            ArrowEntry arrow = obj.GetComponent<ArrowEntry>();
            arrow.Initialize(_boardSystem, data, _pool, _arrowColors[i % _arrowColors.Length]);
            _arrows.Add(arrow);
        }
    }
}
