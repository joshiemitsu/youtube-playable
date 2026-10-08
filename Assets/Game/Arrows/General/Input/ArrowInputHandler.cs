using R3;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class ArrowInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private InputActionReference _tapAction;
    [SerializeField] private LayerMask _arrowLayer;

    public readonly Subject<ArrowEntry> OnArrowTapped = new();

    private bool _isEnabled = true;

    public void SetEnabled(bool p_isEnabled) => _isEnabled = p_isEnabled;

    private void Start()
    {
        SetEnabled(true);
    }

    private void OnEnable()
    {
        _tapAction.action.performed += OnTap; 
        _tapAction.action.Enable();
    }

    private void OnDisable()
    {
        _tapAction.action.performed -= OnTap;
    }

    private void OnDestroy()
    {
        OnArrowTapped.Dispose();
    }

    private void OnTap(InputAction.CallbackContext p_context)
    {
        Debug.Log("On Tap Pressed: " + _isEnabled);
        if(!_isEnabled) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector2 worldPos = _camera.ScreenToWorldPoint(screenPos);

        Collider2D hit = Physics2D.OverlapPoint(worldPos, _arrowLayer);
        if (hit != null && hit.TryGetComponent(out ArrowSubUnit subUnit))
        {
            Debug.Log("Object Hit: " + subUnit.Parent.gameObject.name);
            OnArrowTapped.OnNext(subUnit.Parent);
        }
    }
}
