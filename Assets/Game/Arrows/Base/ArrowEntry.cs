using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RapidPro.Pooling;
using UnityEngine;

public class ArrowData
{
    public Vector2 Direction;
    public Vector2 StartPoint;
    public LineRenderer LineRender;
    public List<Vector2> Points;
}

[RequireComponent(typeof(LineRenderer))]
public class ArrowEntry : MonoBehaviour, IPoolable
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

    public bool IsActive => _isActive;
    private void SetIsActive(bool p_isActive) => _isActive = p_isActive;
    private bool _isActive = false;

    private IPoolService _pool;

    public void Initialize(GameBoardSystem p_boardSystem, ArrowData p_data, IPoolService p_pool, Color p_color)
    {
        _arrowData = p_data;
        _boardSystem = p_boardSystem;
        _pool = p_pool;

        // Intialize the components
        _moveHandler.Initialize(this, p_boardSystem);

        int FIRST_IDX = 0;
        this.transform.position = _boardSystem.GetWorldPosition(p_data.Points[FIRST_IDX]);

        _isActive = true;

        SpawnParts(p_color);
    }

    // Tells what a tap would do, without moving anything
    public ArrowTapResult PreviewTap()
    {
        if (_moveHandler.IsMoving() || !_isActive)
        {
            return ArrowTapResult.Ignored;
        }

        List<Vector2> path = _boardSystem.GetPathCells(_arrowData.Direction, _arrowData.StartPoint);
        Vector2 lastcell = path.Count > 0 ? path[^1] : _arrowData.StartPoint;

        return _boardSystem.CanExitLevel(lastcell, _arrowData.Direction) ? ArrowTapResult.Exited : ArrowTapResult.Blocked;
    }

    // Moves the arrow and finishes when its animation ends
    public async UniTask<ArrowTapResult> OnPressed()
    {
        ArrowTapResult result = PreviewTap();
        if (result == ArrowTapResult.Ignored)
        {
            return result;
        }

        List<Vector2> path = _boardSystem.GetPathCells(_arrowData.Direction, _arrowData.StartPoint);
        bool canExitLevel = result == ArrowTapResult.Exited;

        if (canExitLevel)
        {
            CleanUp();
        }

        await _moveHandler.Move(path, _arrowData.Direction, canExitLevel);
        RefreshLine();

        return result;
    }

    public void CleanUp()
    {
        _boardSystem.ReleaseSlots(_arrowData);
        SetIsActive(false);
    }

    // Returns the head and body parts to the pool
    public void DespawnParts()
    {
        foreach (var subUnit in _subUnitList)
        {
            _pool.Despawn(subUnit.gameObject);
        }
        _subUnitList.Clear();
    }

    public void OnSpawn() { }

    public void OnDespawn()
    {
        SetIsActive(false);
    }

    // Spawn the parts of the arrow
    private void SpawnParts(Color p_color)
    {
        int headIdx = 0;

        _arrowData.LineRender = GetComponent<LineRenderer>();
        _arrowData.LineRender.useWorldSpace = true;
        _arrowData.LineRender.positionCount = _arrowData.Points.Count;
        _arrowData.LineRender.startColor = p_color;
        _arrowData.LineRender.endColor = p_color;

        List<Vector2> points = _arrowData.Points;

        for (int i = 0; i < points.Count; i++)
        {
            GameObject prefab = (i == headIdx) ? _headObj : _bodyObj;
            Vector2 pos = _boardSystem.GetWorldPosition(points[i]);

            GameObject obj = _pool.Spawn(prefab, pos, Quaternion.identity);
            obj.transform.SetParent(_parent, true);

            ArrowSubUnit subUnit = obj.GetComponent<ArrowSubUnit>();
            subUnit.Initialize(this, _arrowData.Points[i]);
            subUnit.SetColor(p_color);

            if (i == headIdx)
            {
                subUnit.FaceDirection(GetHeadFacing());
            }

            _subUnitList.Add(subUnit);
            _arrowData.LineRender.SetPosition(i, pos);
        }
    }

    // The head points away from the second segment, like the tip of a drawn arrow.
    // A one-cell arrow has no second segment, so it uses its move direction.
    private Vector2 GetHeadFacing()
    {
        List<Vector2> points = _arrowData.Points;
        return points.Count > 1 ? points[0] - points[1] : _arrowData.Direction;
    }

    private void RefreshLine()
    {
        for (int i = 0; i < _subUnitList.Count; i++)
        {
            _arrowData.LineRender.SetPosition(i, _subUnitList[i].transform.position);
        }
    }

    private void LateUpdate()
    {
        if(!_moveHandler.IsMoving())
        {
            return;
        }

        RefreshLine();
    }
}
