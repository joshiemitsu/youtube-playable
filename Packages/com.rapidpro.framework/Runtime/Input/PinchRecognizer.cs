using System.Collections.Generic;
using R3;
using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// Fires pinch events while exactly two fingers are down and their distance changes, and for the
    /// scroll wheel on desktop and WebGL. <see cref="PinchedOut"/> = fingers apart (zoom in),
    /// <see cref="PinchedIn"/> = fingers together (zoom out).
    /// </summary>
    public class PinchRecognizer : IGestureRecognizer
    {
        private readonly Subject<PinchEvent> _pinched = new Subject<PinchEvent>();
        private readonly List<TrackedPointer> _down = new List<TrackedPointer>(2);
        private bool _isPinching;
        private int _firstId;
        private int _secondId;
        private float _startDistance;
        private float _lastDistance;

        /// <summary>Creates the recognizer.</summary>
        public PinchRecognizer()
        {
            PinchedIn = _pinched.Where(pinch => pinch.Delta < 0f);
            PinchedOut = _pinched.Where(pinch => pinch.Delta > 0f);
        }

        /// <summary>Every pinch change, in or out. Use <see cref="PinchEvent.Delta"/> to zoom smoothly.</summary>
        public Observable<PinchEvent> Pinched => _pinched;

        /// <summary>Fingers moving together (zoom out), or scrolling down.</summary>
        public Observable<PinchEvent> PinchedIn { get; }

        /// <summary>Fingers moving apart (zoom in), or scrolling up.</summary>
        public Observable<PinchEvent> PinchedOut { get; }

        /// <inheritdoc />
        public void Process(PointerFrame frame, InputConfig config)
        {
            _down.Clear();
            foreach (var pointer in frame.Pointers)
            {
                if (pointer.IsDown && !PointerFilter.IsIgnored(pointer, config))
                    _down.Add(pointer);
            }

            if (_down.Count == 2)
            {
                // Lowest id first, so the pair is recognised as the same pinch whatever order they arrive in.
                bool inOrder = _down[0].Id < _down[1].Id;
                ProcessTwoFingers(inOrder ? _down[0] : _down[1], inOrder ? _down[1] : _down[0], frame, config);
            }
            else
                _isPinching = false;

            if (_down.Count < 2 && frame.Scroll != 0f && config.ScrollPinchAmount > 0f && !(config.IgnorePointersOverUI && frame.CursorOverUI))
                _pinched.OnNext(new PinchEvent(frame.CursorPosition, frame.Scroll * config.ScrollPinchAmount, 1f));
        }

        /// <inheritdoc />
        public void Reset()
        {
            _isPinching = false;
        }

        private void ProcessTwoFingers(TrackedPointer first, TrackedPointer second, PointerFrame frame, InputConfig config)
        {
            float distance = Mathf.Max(Vector2.Distance(first.Position, second.Position), 1f);

            bool sameFingers = _isPinching && first.Id == _firstId && second.Id == _secondId;
            if (!sameFingers)
            {
                _isPinching = true;
                _firstId = first.Id;
                _secondId = second.Id;
                _startDistance = distance;
                _lastDistance = distance;
                return;
            }

            float change = (distance - _lastDistance) / Mathf.Min(frame.ScreenSize.x, frame.ScreenSize.y);
            if (Mathf.Abs(change) < config.PinchMinChange || change == 0f)
                return;

            var center = (first.Position + second.Position) * 0.5f;
            _pinched.OnNext(new PinchEvent(center, change, distance / _startDistance));
            _lastDistance = distance;
        }
    }
}
