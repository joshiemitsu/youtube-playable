using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// Thresholds for every gesture. Create one with <b>Assets &gt; Create &gt; RapidPro &gt; Input Config</b>
    /// and assign it to the <see cref="InputService"/>. Values can be tweaked in Play Mode.
    /// </summary>
    /// <remarks>
    /// Distances are fractions of the screen's shorter side, so a gesture feels the same on a small phone,
    /// a tablet, a WebGL page or a desktop window. 0.02 on a 1080 x 1920 phone is about 22 pixels.
    /// </remarks>
    [CreateAssetMenu(fileName = "InputConfig", menuName = "RapidPro/Input Config")]
    public class InputConfig : ScriptableObject
    {
        [Header("Tap and hold")]
        [Tooltip("How far a finger may move and still count as a tap or hold. Beyond this it becomes a drag. Fraction of the screen's shorter side.")]
        [Range(0.005f, 0.1f)]
        [SerializeField] private float _dragThreshold = 0.02f;

        [Tooltip("Seconds a finger must stay down, without moving, to count as a hold. Shorter presses are taps.")]
        [Range(0.1f, 2f)]
        [SerializeField] private float _holdTime = 0.5f;

        [Tooltip("Most seconds between two taps for them to count as a double tap.")]
        [Range(0.1f, 0.6f)]
        [SerializeField] private float _doubleTapTime = 0.3f;

        [Tooltip("How far apart two taps may be and still count as a double tap. Fraction of the screen's shorter side.")]
        [Range(0.01f, 0.2f)]
        [SerializeField] private float _doubleTapDistance = 0.06f;

        [Header("Flick")]
        [Tooltip("Lowest speed for a released drag to count as a flick (swipe), in screen shorter-sides per second.")]
        [Range(0.2f, 5f)]
        [SerializeField] private float _flickMinSpeed = 1.5f;

        [Tooltip("Longest a flick may take from touch to release, in seconds. Slower moves are plain drags.")]
        [Range(0.05f, 1f)]
        [SerializeField] private float _flickMaxTime = 0.35f;

        [Header("Pinch")]
        [Tooltip("Smallest change in finger distance per frame that counts as pinching. Fraction of the screen's shorter side.")]
        [Range(0f, 0.02f)]
        [SerializeField] private float _pinchMinChange = 0.002f;

        [Tooltip("How much one scroll-wheel notch pinches, on desktop and WebGL. 0 turns wheel pinching off.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _scrollPinchAmount = 0.1f;

        [Header("UI")]
        [Tooltip("Ignore touches that start on a UI element, so pressing a button does not also tap the game.")]
        [SerializeField] private bool _ignorePointersOverUI = true;

        /// <summary>Most movement for a tap or hold, as a fraction of the screen's shorter side.</summary>
        public float DragThreshold => _dragThreshold;

        /// <summary>Seconds before a still press becomes a hold.</summary>
        public float HoldTime => _holdTime;

        /// <summary>Most seconds between taps of a double tap.</summary>
        public float DoubleTapTime => _doubleTapTime;

        /// <summary>Most distance between taps of a double tap, as a fraction of the screen's shorter side.</summary>
        public float DoubleTapDistance => _doubleTapDistance;

        /// <summary>Lowest flick speed, in screen shorter-sides per second.</summary>
        public float FlickMinSpeed => _flickMinSpeed;

        /// <summary>Longest flick duration in seconds.</summary>
        public float FlickMaxTime => _flickMaxTime;

        /// <summary>Smallest pinch change per frame, as a fraction of the screen's shorter side.</summary>
        public float PinchMinChange => _pinchMinChange;

        /// <summary>Pinch amount per scroll-wheel notch.</summary>
        public float ScrollPinchAmount => _scrollPinchAmount;

        /// <summary>True when touches that start on UI are ignored.</summary>
        public bool IgnorePointersOverUI => _ignorePointersOverUI;
    }
}
