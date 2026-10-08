using System.Collections.Generic;
using ObservableCollections;
using R3;
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

    [Header("Subjects")]
    public readonly Subject<(int width, int height)> SpawnGrid = new();
    public readonly Subject<int[,]> OnBoardChanged = new();

    private int[,] _boardSlots;

    private Vector2 _direction;
    private Vector2 _startPoint;
    private Vector2 _currentPoint;

    public void Start()
    {
        Debug.Log("GBSystem Awake");
        Init();
    }

    private void Init()
    {
        Debug.Log("GBSystem Init");
        _boardSlots = new int[WIDTH, HEIGHT];
        SpawnGrid.OnNext((WIDTH, HEIGHT));
    }

    public void OnDisable()
    {
        SpawnGrid.Dispose();
        OnBoardChanged.Dispose();
    }

    // Check if the arrows can move given the said direction and returns world position points
    public List<Vector2> GetPathCells(Vector2 p_direction, Vector2 p_startPoint)
    {
        // set initial values
        _startPoint = p_startPoint;
        _currentPoint = p_startPoint + p_direction;
        _direction = p_direction;

        List<Vector2> pathToExit = new List<Vector2>();

        while (true)
        {
            Debug.Log("Getting Slot in : " + _currentPoint);

            bool isOutOfBounds = _currentPoint.x < 0 || _currentPoint.y < 0 || 
                                _currentPoint.x >= WIDTH || _currentPoint.y >= HEIGHT;
            if (isOutOfBounds)
            {
                Debug.Log("Reached Out of bounds");
                return pathToExit;
            }

            bool didReachedEnding = GetSlot(_currentPoint) == (int)SlotType.RESTRICTED;
            if (didReachedEnding)
            {
                Debug.Log("Reached Ending");
                return pathToExit;
            }

            bool isValidSlot = GetSlot(_currentPoint) == (int)SlotType.FREE;
            if (!isValidSlot)
            {
                Debug.Log("is not ValidSlot");
                return null;
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

    public int GetSlot(Vector2 p_slot)
        => _boardSlots[(int)p_slot.x, (int)p_slot.y];

    public void SetSlot(Vector2 p_slot, int p_value)
        => _boardSlots[(int) p_slot.x, (int) p_slot.y] = p_value;

    public Vector2 GetWorldPosition(Vector2 p_slotPos)
    {
        float xOffset = (WIDTH - 1) * SPACING * 0.5f;
        float yOffset = (HEIGHT - 1) * SPACING * 0.5f;
        return (Vector2)_worldParent.position + new Vector2(
                        p_slotPos.x * SPACING - xOffset,
                        p_slotPos.y * SPACING - yOffset);
    }
}
