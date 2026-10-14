using System.Collections.Generic;
using RapidPro.Core;
using UnityEngine;

public enum SlotType
{
    FREE = 0,
    OCCUPIED = 1,
    RESTRICTED
}

public class GameBoardSystem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int WIDTH = 10;
    [SerializeField] private int HEIGHT = 10;

    [Header("Spacing")]
    [SerializeField] private float SPACING = 1;

    [Header("World Parent")]
    [SerializeField] private Transform _worldParent;
    public Transform WorldParent => _worldParent;

    public int Width => WIDTH;
    public int Height => HEIGHT;

    private int[,] _boardSlots;

    private Vector2 _direction;
    private Vector2 _startPoint;
    private Vector2 _currentPoint;

    // Builds an empty board for a level. Cells outside the shape are RESTRICTED.
    public void Setup(int p_width, int p_height, IEnumerable<Vector2Int> p_restrictedCells)
    {
        WIDTH = p_width;
        HEIGHT = p_height;
        _boardSlots = new int[WIDTH, HEIGHT];

        foreach (var cell in p_restrictedCells)
        {
            SetSlot(new Vector2(cell.x, cell.y), (int)SlotType.RESTRICTED);
        }
    }

    // Check if the arrows can move given the said direction and returns board position
    public List<Vector2> GetPathCells(Vector2 p_direction, Vector2 p_startPoint)
    {
        // set initial values
        _startPoint = p_startPoint;
        _currentPoint = p_startPoint + p_direction;
        _direction = p_direction;

        List<Vector2> pathToExit = new List<Vector2>();

        while (true)
        {
            bool isOutOfBounds = _currentPoint.x < 0 || _currentPoint.y < 0 ||
                                _currentPoint.x >= WIDTH || _currentPoint.y >= HEIGHT;
            if (isOutOfBounds)
            {
                return pathToExit;
            }

            bool didReachedEnding = GetSlot(_currentPoint) == (int)SlotType.RESTRICTED;
            if (didReachedEnding)
            {
                return pathToExit;
            }

            bool isValidSlot = GetSlot(_currentPoint) == (int)SlotType.FREE;
            if (!isValidSlot)
            {
                Log.Info("Path blocked at " + _currentPoint);
                return pathToExit;
            }

            pathToExit.Add(_currentPoint);

            _currentPoint += p_direction;
        }
    }

    // Release the slots used by the arrow that is already cleared
    public void ReleaseSlots(ArrowData _data)
    {
        for(int i = 0; i < _data.Points.Count; i++)
        {
            Vector2 point = new Vector2(_data.Points[i].x, _data.Points[i].y);
            SetSlot(point, (int)SlotType.FREE);
        }
    }

    public bool CanExitLevel(Vector2 p_lastCell, Vector2 p_direction)
    {
        return GetSlot(p_lastCell + p_direction) == (int)SlotType.RESTRICTED;
    }

    public int GetSlot(Vector2 p_slot)
    {
        // If slot is in edge
        if(p_slot.x >= WIDTH || p_slot.x < 0
            || p_slot.y >= HEIGHT || p_slot.y < 0)
        {
            return (int)SlotType.RESTRICTED;
        }

        return _boardSlots[(int)p_slot.x, (int)p_slot.y];
    }

    public void SetSlot(Vector2 p_slot, int p_value)
        => _boardSlots[(int) p_slot.x, (int) p_slot.y] = p_value;

    public bool IsPlayable(Vector2 p_slot) => GetSlot(p_slot) != (int)SlotType.RESTRICTED;

    public Vector2 GetWorldPosition(Vector2 p_slotPos)
    {
        float xOffset = (WIDTH - 1) * SPACING * 0.5f;
        float yOffset = (HEIGHT - 1) * SPACING * 0.5f;
        return (Vector2)_worldParent.position + new Vector2(
                        p_slotPos.x * SPACING - xOffset,
                        p_slotPos.y * SPACING - yOffset);
    }

    // World area covered by the playable cells, so the camera can fit the shape (not the empty corners)
    public Bounds GetPlayableWorldBounds()
    {
        bool hasCell = false;
        Bounds bounds = new Bounds(_worldParent.position, Vector3.zero);
        Vector3 cellSize = new Vector3(SPACING, SPACING, 0f);

        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                if (!IsPlayable(new Vector2(x, y))) continue;

                var cellBounds = new Bounds(GetWorldPosition(new Vector2(x, y)), cellSize);
                if (hasCell) bounds.Encapsulate(cellBounds);
                else { bounds = cellBounds; hasCell = true; }
            }
        }
        return bounds;
    }
}
