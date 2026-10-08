using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Handles the arrow movements
/// </summary>
public class ArrowMoveHandler: MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _speed = 0;

    [Tooltip("Extra steps after the tail leaves the board, so the arrow fully exits the screen.")]
    [SerializeField] private int _bufferSteps = 10;

    private bool _isMoving = false;

    private GameBoardSystem _boardSystem = null;

    private Vector2 _direction;

    private ArrowEntry _parent = null;

    private bool _isPathToExitValid = false;

    private List<Vector2> _pathToExit = new List<Vector2>(); 

    private List<ArrowSubUnit> _subUnits = new List<ArrowSubUnit>();

    public void Initialize(ArrowEntry p_entry, GameBoardSystem p_system)
    {
        _parent = p_entry;
        _subUnits = p_entry.SubUnitList;
        _boardSystem = p_system;
    }

    public bool IsMoving()
    {
        return _isMoving;
    }

    public UniTask Move(List<Vector2> p_pathCells, Vector2 p_direction, bool p_isValid)
    {
        _pathToExit = p_pathCells;
        _direction = p_direction;
        _isPathToExitValid = p_isValid;

        _isMoving = true;
        return DoMoveSequence();
    }

    private async UniTask DoMoveSequence()
    {
        int totalSteps = _isPathToExitValid ? _pathToExit.Count + _subUnits.Count + _bufferSteps : 1;

        for (int step = 0; step < totalSteps; step++)
        {
            Vector2 leaderCell = GetHeadCell(step);
            foreach (var unit in _subUnits)
            {
                Vector2 previousCell = unit.CurrentPoint;
                unit.MoveSubUnit(leaderCell, _boardSystem.GetWorldPosition(leaderCell), _speed, _isPathToExitValid);

                if (!_isPathToExitValid) break; // blocked: only the head bumps
                leaderCell = previousCell;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_speed));
        }

        _isMoving = false;
    }

    private Vector2 GetHeadCell(int step)
    {
        if (step < _pathToExit.Count) return _pathToExit[step];
        Vector2 last = _pathToExit.Count > 0 ? _pathToExit[^1] : _subUnits[0].CurrentPoint;

        return last + _direction * (step - _pathToExit.Count + 1);
    }
}