using UnityEngine;

namespace RapidPro.Display
{
    /// <summary>
    /// Moves and sizes the camera so the whole level fits the screen on any device: tall phones,
    /// tablets, a resized WebGL page or a desktop window. Orthographic cameras change their size;
    /// perspective cameras move closer or further along their view direction (the z axis for a
    /// camera looking straight ahead).
    /// </summary>
    /// <remarks>
    /// Set the level area in the Inspector, or from code after a level loads:
    /// <code>
    /// _cameraFitter.SetBounds(new Bounds(boardCenter, new Vector3(columns, rows, 0f)));
    /// </code>
    /// It refits by itself whenever the screen size, safe area, camera aspect or any setting changes.
    /// The camera's rotation is never changed.
    /// </remarks>
    [RequireComponent(typeof(Camera))]
    public class CameraFitter : MonoBehaviour
    {
        [Header("Level")]
        [Tooltip("World area that must stay on screen. Set it here, or call SetBounds after a level loads.")]
        [SerializeField] private Bounds _bounds = new Bounds(Vector3.zero, new Vector3(10f, 10f, 0f));

        [Tooltip("Extra world units kept free around the level on every side.")]
        [Min(0f)]
        [SerializeField] private float _padding = 0.5f;

        [Header("Fit")]
        [Tooltip("Fit Inside keeps the whole level visible. Weighted blends between fitting the width and the height.")]
        [SerializeField] private CameraFitMode _mode = CameraFitMode.FitInside;

        [Tooltip("Weighted mode only. 0 = fit the level's width, 1 = fit its height.")]
        [Range(0f, 1f)]
        [SerializeField] private float _widthOrHeight = 0.5f;

        [Tooltip("Where the level sits when the screen has spare room, from (0,0) bottom-left to (1,1) top-right. (0.5, 0.5) = centre, (0.5, 0) = bottom.")]
        [SerializeField] private Vector2 _anchor = new Vector2(0.5f, 0.5f);

        [Header("Screen space")]
        [Tooltip("Keep the level clear of notches and rounded corners.")]
        [SerializeField] private bool _useSafeArea = true;

        [Tooltip("Part of the screen height kept free at the top, e.g. for the HUD.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _reservedTop;

        [Tooltip("Part of the screen height kept free at the bottom, e.g. for a banner ad.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _reservedBottom;

        private Camera _camera;
        private ScreenSnapshot _lastScreen;
        private float _lastAspect;
        private bool _isDirty = true;

        /// <summary>The world area kept on screen.</summary>
        public Bounds Bounds => _bounds;

        private Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = GetComponent<Camera>();
                return _camera;
            }
        }

        /// <summary>Sets the world area to keep on screen, e.g. the board after a level loads, and refits.</summary>
        public void SetBounds(Bounds bounds)
        {
            _bounds = bounds;
            Refit();
        }

        /// <summary>Fits the camera right away. Also available from the component's ⋮ menu as "Fit Now".</summary>
        [ContextMenu("Fit Now")]
        public void Refit()
        {
            var screen = ScreenSnapshot.Current;
            Fit(screen, Camera.aspect);
        }

        /// <summary>Lets tests change settings without the Inspector.</summary>
        internal void Configure(CameraFitMode mode, float widthOrHeight, Vector2 anchor, float padding,
            bool useSafeArea, float reservedTop, float reservedBottom)
        {
            _mode = mode;
            _widthOrHeight = widthOrHeight;
            _anchor = anchor;
            _padding = padding;
            _useSafeArea = useSafeArea;
            _reservedTop = reservedTop;
            _reservedBottom = reservedBottom;
        }

        /// <summary>The usable part of the screen: the safe area (if used) minus the reserved top and bottom.</summary>
        internal Rect GetViewport(ScreenSnapshot screen)
        {
            Rect area = _useSafeArea ? screen.NormalizedSafeArea : new Rect(0f, 0f, 1f, 1f);
            float bottom = area.yMin + _reservedBottom;
            float top = area.yMax - _reservedTop;
            return Rect.MinMaxRect(area.xMin, bottom, area.xMax, Mathf.Max(top, bottom));
        }

        internal void Fit(ScreenSnapshot screen, float aspect)
        {
            var camera = Camera;
            var result = CameraFitMath.Calculate(
                _bounds, transform.rotation, transform.position,
                camera.orthographic, camera.fieldOfView, aspect,
                GetViewport(screen), _padding, _mode, _widthOrHeight, _anchor);

            transform.position = result.Position;
            if (camera.orthographic)
                camera.orthographicSize = result.OrthographicSize;

            _lastScreen = screen;
            _lastAspect = aspect;
            _isDirty = false;
        }

        private void OnEnable()
        {
            _isDirty = true;
        }

        private void OnValidate()
        {
            _isDirty = true;
        }

        private void LateUpdate()
        {
            var screen = ScreenSnapshot.Current;
            float aspect = Camera.aspect;

            if (_isDirty || screen != _lastScreen || !Mathf.Approximately(aspect, _lastAspect))
                Fit(screen, aspect);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);
        }
    }
}
