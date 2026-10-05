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

    [SerializeField] private Vector2 _targetPos;

    private ArrowEntry _parent = null;

    public void Initialize(ArrowEntry p_parent, Vector2 p_startPoint)
    {
        _parent = p_parent;

        _startPoint = this.transform.position;
        _currentPoint = _startPoint;
    }

    public void MoveSubUnit(Vector2 p_targetPos, float p_speed, bool isValidMovement)
    {
        _targetPos = p_targetPos;

        if (isValidMovement)
        {
            this.transform.DOMove(p_targetPos, p_speed).OnComplete(() =>
            {
                _currentPoint = p_targetPos;
            });
        }
        else
        {
            this.transform.DOMove(p_targetPos, p_speed).SetLoops(2, LoopType.Yoyo);
        }
    }

    public void OnMouseDown()
    {
        _parent.OnPressed();
        Debug.Log("On Pressed in Arrow: " + this.gameObject.name);
    }
}
