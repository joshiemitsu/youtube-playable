using Cysharp.Threading.Tasks;
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

    public void Move(List<Vector2> p_pathToExit, bool p_isValid)
    {
        _pathToExit = p_pathToExit;
        _isValid = p_isValid;
        DoMoveSequence().Forget();
    }

    private async UniTaskVoid DoMoveSequence()
    {
        int totalMovementCount = _subUnits.Count + _pathToExit.Count;
        int movementIdx = 1;

        while (totalMovementCount > 0)
        {
            // Iterate to all the sub units and animate things.
            for (int i = 0; i < _subUnits.Count; i++)
            {
                Vector2 targetPos;

                int FIRST_IDX = 0;
                Vector2 targetPoint = new Vector2();
                // if first index, check the next point based on direction otherwise get the point from previous segment
                if (i == FIRST_IDX)
                {
                    targetPoint = _pathToExit[movementIdx];
                    targetPos = targetPoint;
                }
                else
                {
                    int prevUnitIdx = i - 1;
                    targetPoint = _subUnits[prevUnitIdx].CurrentPoint;
                    targetPos = targetPoint;
                }

                float actualSpeed = _speed;
                _subUnits[i].MoveSubUnit(targetPos, actualSpeed, _isValid);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_speed));

            if (movementIdx < _pathToExit.Count - 1)
            {
                movementIdx++;
            }

            totalMovementCount-- ;
        }
    }
}