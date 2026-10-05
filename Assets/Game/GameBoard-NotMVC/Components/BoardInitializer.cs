using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardInitializer : MonoBehaviour
{
    [Header("Main System")]
    [SerializeField] private GameBoardSystem _boardSystem;

    [Header("Prefab")]
    [SerializeField] private GameObject _dotsPrefab;
    [SerializeField] private GameObject _arrowPrefab;

    [Header("World Positions")]
    [SerializeField] private Vector2[,] _worldPosition;

    [Header("GameObject Lists")]
    // create list of arrows for now
    private List<ArrowEntry> _activeArrows = new List<ArrowEntry>();
    private List<ArrowEntry> _inactiveArrow = new List<ArrowEntry>();

    private void Awake()
    {
        var d = Disposable.CreateBuilder();
        _boardSystem.SpawnGrid
            .Subscribe(Size => SpawnBoard(Size.width, Size.height))
            .AddTo(ref d);
        d.RegisterTo(destroyCancellationToken);
    }

    private void SpawnBoard(int p_width, int p_height)
    {
        Debug.Log("Spawn Board called");
        _worldPosition = new Vector2[p_width, p_height]; 

        for (int w = 0; w < p_width; w++)
        {
            for (int h = 0; h < p_height; h++)
            {
                GameObject obj = Instantiate(_dotsPrefab, _boardSystem.WorldParent.transform);

                obj.transform.position = _boardSystem.GetWorldPosition(new Vector2(w,h));

                _worldPosition[w, h] = new Vector2(w, h);
            }
        }

        SpawnArrows();
    }

    private void SpawnArrows()
    {
        GameObject obj = Instantiate(_arrowPrefab, _boardSystem.WorldParent.transform);
        ArrowEntry arrowEntry = obj.GetComponent<ArrowEntry>();

        List<Vector2> posList = new List<Vector2>();
        posList.Add(_worldPosition[1, 0]);
        _boardSystem.SetSlot(new Vector2(1, 0), (int)SlotType.OCCUPIED);

        posList.Add(_worldPosition[2, 0]);
        _boardSystem.SetSlot(new Vector2(2, 0), (int)SlotType.OCCUPIED);

        posList.Add(_worldPosition[3, 0]);
        _boardSystem.SetSlot(new Vector2(3, 0), (int)SlotType.OCCUPIED);

        posList.Add(_worldPosition[3, 1]);
        _boardSystem.SetSlot(new Vector2(3, 1), (int)SlotType.OCCUPIED);

        posList.Add(_worldPosition[3, 2]);
        _boardSystem.SetSlot(new Vector2(3, 2), (int)SlotType.OCCUPIED);

        posList.Add(_worldPosition[4, 2]);
        _boardSystem.SetSlot(new Vector2(4, 2), (int)SlotType.OCCUPIED);

        posList.Add(_worldPosition[5, 2]);
        _boardSystem.SetSlot(new Vector2(5, 2), (int)SlotType.OCCUPIED);

        ArrowData newData = new ArrowData();
        newData.Points = posList;
        newData.Direction = Vector2.left;
        newData.StartPoint = posList[0]; // Get first index as the start position

        arrowEntry.Initialize(_boardSystem, newData);


        GameObject obj2 = Instantiate(_arrowPrefab, _boardSystem.WorldParent.transform);
        ArrowEntry arrowEntry2 = obj2.GetComponent<ArrowEntry>();

        List<Vector2> posList2 = new List<Vector2>();
        posList2.Add(_worldPosition[0, 2]);
        _boardSystem.SetSlot(new Vector2(0, 2), (int)SlotType.OCCUPIED);

        posList2.Add(_worldPosition[0, 1]);
        _boardSystem.SetSlot(new Vector2(0, 1), (int)SlotType.OCCUPIED);

        //posList2.Add(_worldPosition[0, 0]);
        //_boardSystem.SetSlot(new Vector2(0, 0), (int)SlotType.OCCUPIED);

        ArrowData newData2 = new ArrowData();
        newData2.Points = posList2;
        newData2.Direction = Vector2.up;
        newData2.StartPoint = posList2[0]; // Get first index as the start position

        arrowEntry2.Initialize(_boardSystem, newData2);
    }
}
