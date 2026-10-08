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

    public readonly ObservableList<Vector2> Points = new();
    public readonly Subject<Vector2> OnMove = new();

    private bool _isActive = false;

    public void Initialize(GameBoardSystem p_boardSystem, ArrowData p_data)
    {
        // Intialize the components
        _moveHandler.Initialize(this, p_boardSystem);

        OnMove.Subscribe(OnMoveChanged);
        OnMove.OnNext(Vector2.zero);

        int FIRST_IDX = 0;
        this.transform.position = p_data.Points[FIRST_IDX];

        _arrowData = p_data;
        _boardSystem = p_boardSystem;

        SpawnParts();
    }

    public UniTask OnPressed()
    {
        if(!_moveHandler.IsMoving() || !_isActive)
        {
           return UniTask.CompletedTask;
        }

        List<Vector2> path = _boardSystem.GetPathCells(_arrowData.Direction, _arrowData.StartPoint);
        bool canMove = path != null;

        if (canMove)
        {
            _boardSystem.ReleaseSlots(_arrowData);
            return _moveHandler.Move(path, _arrowData.Direction, true);
        }

        return _moveHandler.Move(new List<Vector2> { _arrowData.StartPoint + _arrowData.Direction },
                          _arrowData.Direction, false);
    }

    public void CleanUp()
    {
        _isActive = false;
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
        for (int i = 0; i < _subUnitList.Count; i++)
        {
            _arrowData.LineRender.SetPosition(i, _subUnitList[i].transform.position);
        }
    }

    private void OnMoveChanged(Vector2 p_direction)
    {

    }
} 
