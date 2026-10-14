using System.Collections.Generic;
using R3;
using RapidPro.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RapidPro.Input
{
    /// <summary>
    /// Default <see cref="IInputService"/>. Each frame it reads the pointer source, tracks every finger,
    /// and passes the frame to each gesture recognizer in turn.
    /// </summary>
    /// <remarks>
    /// Put it in the Bootstrap scene, assign an <see cref="InputConfig"/>, and register it:
    /// <code>
    /// [SerializeField] private InputService _input;
    /// Services.Register&lt;IInputService&gt;(_input);
    /// </code>
    /// Ways to extend it, from smallest to biggest:
    /// <list type="number">
    /// <item>Tune thresholds in the <see cref="InputConfig"/> asset.</item>
    /// <item>Add a custom gesture: implement <see cref="IGestureRecognizer"/> and call <see cref="AddRecognizer"/>.</item>
    /// <item>Change where input comes from (replays, tutorials, new devices): implement
    /// <see cref="IPointerSource"/> and call <see cref="SetSource"/>.</item>
    /// </list>
    /// Uses unscaled time, so gestures work while the game is paused. Use <see cref="Block"/> to stop them.
    /// </remarks>
    public class InputService : MonoBehaviour, IInputService
    {
        [Tooltip("Gesture thresholds. Create one with Assets > Create > RapidPro > Input Config. Defaults are used when empty.")]
        [SerializeField] private InputConfig _config;

        private readonly PressRecognizer _press = new PressRecognizer();
        private readonly TapRecognizer _tap = new TapRecognizer();
        private readonly HoldRecognizer _hold = new HoldRecognizer();
        private readonly DragRecognizer _drag = new DragRecognizer();
        private readonly PinchRecognizer _pinch = new PinchRecognizer();
        private List<IGestureRecognizer> _recognizers;

        private readonly HashSet<string> _blockReasons = new HashSet<string>();
        private readonly ReactiveProperty<bool> _isBlocked = new ReactiveProperty<bool>(false);

        private readonly List<RawPointer> _raw = new List<RawPointer>();
        private readonly PointerFrame _frame = new PointerFrame();
        private PointerTracker _tracker;
        private IPointerSource _source;
        private bool _waitForRelease;

        // Built-in gestures first, then custom ones in the order they were added.
        private List<IGestureRecognizer> Recognizers
        {
            get
            {
                if (_recognizers == null)
                    _recognizers = new List<IGestureRecognizer> { _press, _tap, _hold, _drag, _pinch };
                return _recognizers;
            }
        }

        /// <inheritdoc />
        public Observable<PressEvent> Pressed => _press.Pressed;

        /// <inheritdoc />
        public Observable<PressEvent> Released => _press.Released;

        /// <inheritdoc />
        public Observable<TapEvent> Tapped => _tap.Tapped;

        /// <inheritdoc />
        public Observable<TapEvent> DoubleTapped => _tap.DoubleTapped;

        /// <inheritdoc />
        public Observable<HoldEvent> Held => _hold.Held;

        /// <inheritdoc />
        public Observable<DragEvent> DragStarted => _drag.DragStarted;

        /// <inheritdoc />
        public Observable<DragEvent> Dragged => _drag.Dragged;

        /// <inheritdoc />
        public Observable<DragEvent> DragEnded => _drag.DragEnded;

        /// <inheritdoc />
        public Observable<FlickEvent> Flicked => _drag.Flicked;

        /// <inheritdoc />
        public Observable<PinchEvent> Pinched => _pinch.Pinched;

        /// <inheritdoc />
        public Observable<PinchEvent> PinchedIn => _pinch.PinchedIn;

        /// <inheritdoc />
        public Observable<PinchEvent> PinchedOut => _pinch.PinchedOut;

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<bool> IsBlocked => _isBlocked;

        /// <summary>The thresholds in use. Creates a default config when none is assigned.</summary>
        public InputConfig Config
        {
            get
            {
                if (_config == null)
                    _config = ScriptableObject.CreateInstance<InputConfig>();
                return _config;
            }
        }

        /// <inheritdoc />
        public void Block(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                Log.Warning("InputService: Block needs a reason, e.g. \"popup\".", this);
                return;
            }

            _blockReasons.Add(reason);
            _isBlocked.Value = true;
        }

        /// <inheritdoc />
        public void Unblock(string reason)
        {
            if (reason == null || !_blockReasons.Remove(reason))
                return;

            _isBlocked.Value = _blockReasons.Count > 0;
        }

        /// <inheritdoc />
        public void AddRecognizer(IGestureRecognizer recognizer)
        {
            if (recognizer != null && !Recognizers.Contains(recognizer))
                Recognizers.Add(recognizer);
        }

        /// <inheritdoc />
        public void RemoveRecognizer(IGestureRecognizer recognizer)
        {
            Recognizers.Remove(recognizer);
        }

        /// <summary>
        /// Replaces where input comes from, e.g. a replay or tutorial source. Gestures in progress are dropped.
        /// </summary>
        public void SetSource(IPointerSource source)
        {
            _source = source;
            ResetGestures();
        }

        /// <summary>Lets tests assign a config without the Inspector.</summary>
        internal void SetConfig(InputConfig config) => _config = config;

        /// <summary>Runs one input frame. Called by <see cref="Update"/>, and by tests with a fake clock.</summary>
        internal void Tick(float time)
        {
            if (_source == null)
                _source = new InputSystemPointerSource();
            if (_tracker == null)
                _tracker = new PointerTracker(IsOverUI);

            _raw.Clear();
            _source.ReadPointers(_raw);

            if (_isBlocked.Value)
            {
                ResetGestures();
                _waitForRelease = true;
                return;
            }

            // Fingers that were already down when input unblocked must not turn into taps.
            if (_waitForRelease)
            {
                if (_raw.Count > 0)
                    return;
                _waitForRelease = false;
            }

            _frame.Set(time, _source.ScreenSize, _source.ReadScroll(), _source.ReadCursorPosition(), IsOverUI(RawPointer.MouseId));
            _tracker.Update(_raw, _frame);

            var config = Config;
            var recognizers = Recognizers;
            for (int i = 0; i < recognizers.Count; i++)
                recognizers[i].Process(_frame, config);
        }

        private void Update()
        {
            Tick(Time.unscaledTime);
        }

        private void OnDestroy()
        {
            _isBlocked.Dispose();
        }

        private void ResetGestures()
        {
            _tracker?.Clear();
            foreach (var recognizer in Recognizers)
                recognizer.Reset();
        }

        private static bool IsOverUI(int pointerId)
        {
            var eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject(pointerId);
        }
    }
}
