using System.Collections.Generic;
using ObservableCollections;
using R3;
using UnityEngine;

public class GameBoardSystem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int WIDTH = 10;
    [SerializeField] private int HEIGHT = 10;

    [Header("Subjects")]
    public readonly ReplaySubject<(int width, int height)> SpawnGrid = new();

    private const int FREE_SLOT = 0;

    private int[,] _boardSlots;

    public void Awake()
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
    }

    public int GetSlot(Vector2 p_slot)
        => _boardSlots[(int)p_slot.x, (int)p_slot.y];

    public void SetSlot(Vector2 p_slot, int p_value)
        => _boardSlots[(int) p_slot.x, (int) p_slot.y] = p_value;
}
