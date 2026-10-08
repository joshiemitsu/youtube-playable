using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using UnityEngine;

public class ArrowData
{
    public Vector2 Direction;
    public Vector2 StartPoint;
    public Vector2 StartWorldPosition;
    public LineRenderer LineRender;
    public List<Vector2> Points;
}

[RequireComponent(typeof(LineRenderer))]
public class ArrowEntry : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private GameBoardSystem _boardSystem;

    [Header("Prefab")]
    [SerializeField] private GameObject _headObj;
    [SerializeField] private GameObject _bodyObj;

    [Header("Parent")]
    [SerializeField] private Transform _parent;

    [Header("Components")]
    [SerializeField] private ArrowMoveHandler _moveHandler;

    [Header("Public Variables")]
    public List<ArrowSubUnit> SubUnitList => _subUnitList;
    private List<ArrowSubUnit> _subUnitList = new List<ArrowSubUnit>();

    public ArrowData ArrowData => _arrowData;
    private ArrowData _arrowData;

    private void SetIsActive(bool p_isActive) => _isActive = p_isActive;
    private bool _isActive = false;

    public void Initialize(GameBoardSystem p_boardSystem, ArrowData p_data)
    {
        // Intialize the components
        _moveHandler.Initialize(this, p_boardSystem);

        int FIRST_IDX = 0;
        this.transform.position = p_data.Points[FIRST_IDX];

        _arrowData = p_data;
        _boardSystem = p_boardSystem;

        _isActive = true;

        SpawnParts();
    }

    public UniTask OnPressed()
    {
        Debug.Log("Arrow On Pressed: " + _moveHandler.IsMoving() + " " + _isActive);
        if(_moveHandler.IsMoving() || !_isActive)
        {
           return UniTask.CompletedTask;
        }

        List<Vector2> path = _boardSystem.GetPathCells(_arrowData.Direction, _arrowData.StartPoint); 

        bool canExitLevel = _boardSystem.CanExitLevel(path[^1], _arrowData.Direction);

        Debug.Log("Arrow On Pressed: " + canExitLevel + " " + _boardSystem.GetSlot(path[^1]));

        if (canExitLevel)
        {
            CleanUp();
        }

        return _moveHandler.Move(path, _arrowData.Direction, canExitLevel);
    }

    public void CleanUp()
    {
        _boardSystem.ReleaseSlots(_arrowData);
        SetIsActive(true);
    }

    // Spawn the parts of the arrow
    private void SpawnParts()
    {
        int headIdx = 0;

        _arrowData.LineRender = GetComponent<LineRenderer>();
        _arrowData.LineRender.positionCount = _arrowData.Points.Count;

        List<Vector2> points = _arrowData.Points;

        for (int i = 0; i < points.Count; i++)
        {
            GameObject prefab = (i == headIdx) ? _headObj : _bodyObj;
            GameObject obj = Instantiate(prefab, _parent.transform);
            obj.transform.position = _boardSystem.GetWorldPosition(points[i]);

            var pos = _boardSystem.GetWorldPosition(_arrowData.Points[i]);
            ArrowSubUnit subUnit = obj.GetComponent<ArrowSubUnit>();
            subUnit.Initialize(this, _arrowData.Points[i]);

            _subUnitList.Add(subUnit);
            _arrowData.LineRender.SetPosition(i, pos);
        }
    }

    private void LateUpdate()
    {
        if(!_isActive)
        {
            return;
        }

        for (int i = 0; i < _subUnitList.Count; i++)
        {
            _arrowData.LineRender.SetPosition(i, _subUnitList[i].transform.position);
        }
    }
} 
