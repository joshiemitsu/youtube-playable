using DG.Tweening;
using UnityEngine;

/// <summary>
/// This is the head and the body parts of the arrow
/// </summary>
public class ArrowSubUnit : MonoBehaviour
{
    [Header("SubUnit Information")]
    public Vector2 StartPoint => _startPoint;
    [SerializeField] private Vector2 _startPoint = Vector2.zero;

    public Vector2 CurrentPoint => _currentPoint;
    [SerializeField] private Vector2 _currentPoint = Vector2.zero;

    public ArrowEntry Parent => _parent;
    private ArrowEntry _parent = null;

    public void Initialize(ArrowEntry p_parent, Vector2 p_startCell)
    {
        _parent = p_parent;
        _startPoint = p_startCell;
        _currentPoint = p_startCell;
    }

    public void MoveSubUnit(Vector2 p_cell, Vector2 p_worldPos, float p_speed, bool isValidMovement)
    {
        if (isValidMovement)
        {
            _currentPoint = p_cell;
            this.transform.DOMove(p_worldPos, p_speed);
        }
        else
        {
            this.transform.DOMove(p_worldPos, p_speed).SetLoops(2, LoopType.Yoyo);
        }
    }
}
