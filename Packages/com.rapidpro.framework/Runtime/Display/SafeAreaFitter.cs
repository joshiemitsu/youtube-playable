using UnityEngine;

namespace RapidPro.Display
{
    /// <summary>
    /// Keeps a UI panel inside the screen's safe area, clear of notches, punch-holes and rounded corners.
    /// Updates by itself when the phone rotates or the window is resized.
    /// </summary>
    /// <remarks>
    /// Put it on a full-screen panel (anchors stretched to all four corners) directly under the Canvas,
    /// and place the screen's content inside that panel. Backgrounds that should run behind the notch
    /// go outside it.
    /// </remarks>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        [Tooltip("Keep content below the notch or status bar.")]
        [SerializeField] private bool _top = true;

        [Tooltip("Keep content above the home indicator.")]
        [SerializeField] private bool _bottom = true;

        [Tooltip("Keep content clear of the left edge (landscape notch).")]
        [SerializeField] private bool _left = true;

        [Tooltip("Keep content clear of the right edge (landscape notch).")]
        [SerializeField] private bool _right = true;

        private RectTransform _rectTransform;
        private ScreenSnapshot _lastScreen;
        private bool _isDirty = true;

        /// <summary>Re-applies the safe area right away, e.g. after changing the edge toggles from code.</summary>
        [ContextMenu("Fit Now")]
        public void Refit()
        {
            Apply(ScreenSnapshot.Current);
        }

        /// <summary>
        /// Returns the anchors that cover the safe area of <paramref name="screen"/>,
        /// ignoring the edges that are switched off.
        /// </summary>
        internal static void CalculateAnchors(ScreenSnapshot screen, bool top, bool bottom, bool left, bool right,
            out Vector2 anchorMin, out Vector2 anchorMax)
        {
            Rect safe = screen.NormalizedSafeArea;

            anchorMin = new Vector2(left ? safe.xMin : 0f, bottom ? safe.yMin : 0f);
            anchorMax = new Vector2(right ? safe.xMax : 1f, top ? safe.yMax : 1f);
        }

        internal void Apply(ScreenSnapshot screen)
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            CalculateAnchors(screen, _top, _bottom, _left, _right, out var anchorMin, out var anchorMax);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            _lastScreen = screen;
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

        private void Update()
        {
            var screen = ScreenSnapshot.Current;
            if (_isDirty || screen != _lastScreen)
                Apply(screen);
        }
    }
}
