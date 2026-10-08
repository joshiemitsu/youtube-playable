using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class ArrowMoveHandler: MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _speed = 0;

    private GameBoardSystem _boardSystem = null;

    private Vector2 _direction;

    private ArrowEntry _parent = null;

    private bool _isValid = false;

    private List<Vector2> _pathToExit = new List<Vector2>(); 

    private List<ArrowSubUnit> _subUnits = new List<ArrowSubUnit>();

    public void Initialize(ArrowEntry p_entry, GameBoardSystem p_system)
    {
        _parent = p_entry;
        _subUnits = p_entry.SubUnitList;
        _boardSystem = p_system;
    }

    public void Move(List<Vector2> p_pathCells, Vector2 p_direction, bool p_isValid)
    {
        _pathToExit = p_pathCells;
        _direction = p_direction;
        _isValid = p_isValid;
        DoMoveSequence().Forget();
    }

    private async UniTaskVoid DoMoveSequence()
    {
        int bufferSteps = 10; // makes sure that arrow shows outside the screen
        int totalSteps = _isValid ? _pathToExit.Count + _subUnits.Count + bufferSteps : 1;

        for (int step = 0; step < totalSteps; step++)
        {
            Debug.Log("Steps loop: " + step);
            Vector2 leaderCell = GetHeadCell(step);
            foreach (var unit in _subUnits)
            {
                Debug.Log(" " + unit.gameObject.name);
                Vector2 previousCell = unit.CurrentPoint;
                unit.MoveSubUnit(leaderCell, _boardSystem.GetWorldPosition(leaderCell), _speed, _isValid);
                if (!_isValid) break;          // blocked: only the head bumps
                leaderCell = previousCell;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_speed));
        }
    }

    private async UniTaskVoid DoReverseMoveSequence()
    {

    }

    private Vector2 GetHeadCell(int step)
    {
        if (step < _pathToExit.Count) return _pathToExit[step];
        Vector2 last = _pathToExit.Count > 0 ? _pathToExit[^1] : _subUnits[0].CurrentPoint;

        return last + _direction * (step - _pathToExit.Count + 1);
    }
}