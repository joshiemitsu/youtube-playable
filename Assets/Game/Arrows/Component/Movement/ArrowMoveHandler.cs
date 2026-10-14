using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the arrow movements
/// </summary>
///

public class ArrowMoveContext
{
    public List<ArrowSubUnit> SubUnits;
    public GameBoardSystem Board;
    public List<Vector2> Path;
    public Vector2 Direction;
    public float StepDuration;
    public int BufferSteps;
}

public class ArrowMoveHandler: MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _speed = 0;

    [Tooltip("Extra steps after the tail leaves the board, so the arrow fully exits the screen.")]
    [SerializeField] private int _bufferSteps = 10;

    [Tooltip("How far the head nudges forward when another arrow is right in front of it, in cells.")]
    [SerializeField] private float _bumpDistance = 0.25f;

    private bool _isMoving = false;

    private GameBoardSystem _boardSystem = null;

    private Vector2 _direction;

    private ArrowEntry _parent = null;

    private List<Vector2> _pathToExit = new List<Vector2>();
    private List<Vector2> _pathToOriginal = new List<Vector2>();

    private List<ArrowSubUnit> _subUnits = new List<ArrowSubUnit>();

    public void Initialize(ArrowEntry p_entry, GameBoardSystem p_system)
    {
        _parent = p_entry;
        _subUnits = p_entry.SubUnitList;
        _boardSystem = p_system;
        _isMoving = false;
    }

    public bool IsMoving()
    {
        return _isMoving;
    }

    public async UniTask Move(List<Vector2> p_pathCells, Vector2 p_direction, bool p_canExitLevel)
    {
        _pathToExit = p_pathCells;
        _direction = p_direction;

        _isMoving = true;

        // try/finally so a failed or cancelled move can never leave the arrow stuck as "moving"
        try
        {
            if (p_canExitLevel)
            {
                await DoMoveSequence();
            }
            else if (p_pathCells.Count == 0)
            {
                await DoBumpSequence();
            }
            else
            {
                await DoMoveThenRevertSequence();
            }
        }
        finally
        {
            _isMoving = false;
        }
    }

    // Move arrow until you exit screen
    private async UniTask DoMoveSequence()
    {
        int totalSteps = _pathToExit.Count + _subUnits.Count + _bufferSteps;

        for (int step = 0; step < totalSteps; step++)
        {
            Vector2 leaderCell = GetHeadCell(step);
            foreach (var unit in _subUnits)
            {
                Vector2 previousCell = unit.CurrentPoint;
                unit.MoveSubUnit(leaderCell, _boardSystem.GetWorldPosition(leaderCell), _speed);
                leaderCell = previousCell;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_speed));
        }
    }

    // Move the arrow until hit something then go back to previous position
    private async UniTask DoMoveThenRevertSequence()
    {
        _pathToOriginal.Clear();

        // Move Forward
        for (int step = 0; step < _pathToExit.Count; step++)
        {
            Vector2 leaderCell = GetHeadCell(step);
            for (int i = 0; i < _subUnits.Count; i++)
            {
                var unit = _subUnits[i];
                Vector2 previousCell = unit.CurrentPoint;
                unit.MoveSubUnit(leaderCell, _boardSystem.GetWorldPosition(leaderCell), _speed);
                leaderCell = previousCell;

                // Get the last unit and save the path
                if (i == _subUnits.Count - 1)
                {
                    _pathToOriginal.Add(previousCell);
                }
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_speed));
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_speed));

        // Reverse to original position
        for (int step = _pathToOriginal.Count - 1; step >= 0; step --)
        {
            Vector2 prevCell = GetTailCell(step);

            // Start from the end until you reached the ending
            for (int i = _subUnits.Count - 1; i >= 0; i--)
            {
                var unit = _subUnits[i];
                Vector2 previousCel = unit.CurrentPoint;
                unit.MoveSubUnit(prevCell, _boardSystem.GetWorldPosition(prevCell), _speed);
                prevCell = previousCel;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_speed));
        }
    }

    // Another arrow is right in front: nudge the head forward and back so the hit is visible
    private async UniTask DoBumpSequence()
    {
        Transform head = _subUnits[0].transform;
        float duration = Mathf.Max(_speed * 4f, 0.15f);

        head.DOPunchPosition(_direction * _bumpDistance, duration, 1, 0f);
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
    }

    private Vector2 GetHeadCell(int step)
    {
        if (step < _pathToExit.Count) return _pathToExit[step];
        return _subUnits[0].CurrentPoint + _direction;
    }

    private Vector2 GetTailCell(int step)
    {
        if (step < _pathToOriginal.Count) return _pathToOriginal[step];
        return _subUnits[^1].CurrentPoint;
    }
}
