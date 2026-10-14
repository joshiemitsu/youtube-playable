using UnityEngine;
using UnityEngine.UI;

namespace RapidPro.Display
{
    /// <summary>
    /// Sets up the Canvas Scaler so the UI fits any resolution, and switches between a portrait and a
    /// landscape layout reference when the screen turns. Works the same on mobile, WebGL and desktop,
    /// and reacts when the window or browser is resized.
    /// </summary>
    /// <remarks>
    /// Add it next to the Canvas Scaler on your Canvas. It overwrites the scaler's mode, reference
    /// resolution and match values, so change them here instead.
    /// </remarks>
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasFitter : MonoBehaviour
    {
        [Tooltip("How the UI scales when the screen shape differs from the reference resolution.")]
        [SerializeField] private CanvasFitMode _mode = CanvasFitMode.FitInside;

        [Header("Portrait (screen taller than wide)")]
        [Tooltip("The resolution the UI was designed for in portrait.")]
        [SerializeField] private Vector2 _portraitReference = new Vector2(1080f, 1920f);

        [Tooltip("Weighted mode only. 0 = match the screen width, 1 = match the screen height.")]
        [Range(0f, 1f)]
        [SerializeField] private float _portraitWidthOrHeight;

        [Header("Landscape (screen wider than tall)")]
        [Tooltip("The resolution the UI was designed for in landscape.")]
        [SerializeField] private Vector2 _landscapeReference = new Vector2(1920f, 1080f);

        [Tooltip("Weighted mode only. 0 = match the screen width, 1 = match the screen height.")]
        [Range(0f, 1f)]
        [SerializeField] private float _landscapeWidthOrHeight = 1f;

        private CanvasScaler _scaler;
        private ScreenSnapshot _lastScreen;
        private bool _isDirty = true;

        /// <summary>Re-applies the settings right away.</summary>
        [ContextMenu("Fit Now")]
        public void Refit()
        {
            Apply(ScreenSnapshot.Current);
        }

        internal CanvasScaler Scaler
        {
            get
            {
                if (_scaler == null)
                    _scaler = GetComponent<CanvasScaler>();
                return _scaler;
            }
        }

        /// <summary>Lets tests pick the mode without the Inspector.</summary>
        internal CanvasFitMode Mode
        {
            set => _mode = value;
        }

        internal void Apply(ScreenSnapshot screen)
        {
            var scaler = Scaler;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = screen.IsPortrait ? _portraitReference : _landscapeReference;
            scaler.matchWidthOrHeight = screen.IsPortrait ? _portraitWidthOrHeight : _landscapeWidthOrHeight;
            scaler.screenMatchMode = ToScreenMatchMode(_mode);

            _lastScreen = screen;
            _isDirty = false;
        }

        private static CanvasScaler.ScreenMatchMode ToScreenMatchMode(CanvasFitMode mode)
        {
            switch (mode)
            {
                case CanvasFitMode.Fill:
                    return CanvasScaler.ScreenMatchMode.Shrink;
                case CanvasFitMode.Weighted:
                    return CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                default:
                    return CanvasScaler.ScreenMatchMode.Expand;
            }
        }

        private void OnEnable()
        {
            _isDirty = true;
        }

        private void OnValidate()
        {
            _isDirty = true;
        }

        private void Update()
        {
            var screen = ScreenSnapshot.Current;
            if (_isDirty || screen != _lastScreen)
                Apply(screen);
        }
    }
}
