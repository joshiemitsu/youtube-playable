using DG.Tweening;
using RapidPro.Pooling;
using UnityEngine;

/// <summary>
/// This is the head and the body parts of the arrow
/// </summary>
public class ArrowSubUnit : MonoBehaviour, IPoolable
{
    [Header("SubUnit Information")]
    public Vector2 StartPoint => _startPoint;
    [SerializeField] private Vector2 _startPoint = Vector2.zero;

    public Vector2 CurrentPoint => _currentPoint;
    [SerializeField] private Vector2 _currentPoint = Vector2.zero;

    [Header("Visual")]
    [Tooltip("The sprite object that turns to face a direction. Leave empty to use the child with the SpriteRenderer. The root, with the collider, never rotates.")]
    [SerializeField] private Transform _visual;

    public ArrowEntry Parent => _parent;
    private ArrowEntry _parent = null;

    public void Initialize(ArrowEntry p_parent, Vector2 p_startCell)
    {
        _parent = p_parent;
        _startPoint = p_startCell;
        _currentPoint = p_startCell;
    }

    public void SetColor(Color p_color)
    {
        foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.color = p_color;
        }
    }

    // Turns only the sprite so it faces p_direction. The collider on the root keeps its shape and rotation.
    // The sprite art points up, hence the -90.
    public void FaceDirection(Vector2 p_direction)
    {
        Transform visual = GetVisual();
        if (visual == null)
        {
            return;
        }

        float angle = Mathf.Atan2(p_direction.y, p_direction.x) * Mathf.Rad2Deg - 90f;
        visual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Transform GetVisual()
    {
        if (_visual != null)
        {
            return _visual;
        }

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        bool isOnChild = spriteRenderer != null && spriteRenderer.transform != transform;
        return isOnChild ? spriteRenderer.transform : null;
    }

    public void MoveSubUnit(Vector2 p_cell, Vector2 p_worldPos, float p_speed)
    {
        _currentPoint = p_cell;
        this.transform.DOMove(p_worldPos, p_speed);
    }

    public void OnSpawn() { }

    // Stop any movement left over from the last level before the part is reused
    public void OnDespawn()
    {
        transform.DOKill();
        _parent = null;
    }
}
