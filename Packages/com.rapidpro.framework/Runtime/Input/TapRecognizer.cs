using R3;
using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// Fires <see cref="Tapped"/> for a quick, still touch and <see cref="DoubleTapped"/> when two taps
    /// land close together in time and place.
    /// </summary>
    /// <remarks>
    /// Tapped fires straight away on every tap, including both taps of a double tap, so single taps never
    /// feel delayed. If a game needs "single tap only, not part of a double tap", wait for the double-tap
    /// time before acting, or write a custom recognizer.
    /// </remarks>
    public class TapRecognizer : IGestureRecognizer
    {
        private readonly Subject<TapEvent> _tapped = new Subject<TapEvent>();
        private readonly Subject<TapEvent> _doubleTapped = new Subject<TapEvent>();
        private float _lastTapTime = float.NegativeInfinity;
        private Vector2 _lastTapPosition;

        /// <summary>A quick touch and release without moving.</summary>
        public Observable<TapEvent> Tapped => _tapped;

        /// <summary>The second of two quick taps close together.</summary>
        public Observable<TapEvent> DoubleTapped => _doubleTapped;

        /// <inheritdoc />
        public void Process(PointerFrame frame, InputConfig config)
        {
            foreach (var pointer in frame.Pointers)
            {
                if (pointer.Phase != PointerPhase.Ended || !PointerFilter.IsSingleFinger(pointer, config))
                    continue;

                bool quick = frame.Time - pointer.StartTime < config.HoldTime;
                bool still = pointer.MaxDistance <= frame.ToPixels(config.DragThreshold);
                if (!quick || !still)
                    continue;

                _tapped.OnNext(new TapEvent(pointer.Position, 1));

                bool soonAfterLastTap = frame.Time - _lastTapTime <= config.DoubleTapTime;
                bool nearLastTap = Vector2.Distance(pointer.Position, _lastTapPosition) <= frame.ToPixels(config.DoubleTapDistance);

                if (soonAfterLastTap && nearLastTap)
                {
                    _doubleTapped.OnNext(new TapEvent(pointer.Position, 2));
                    _lastTapTime = float.NegativeInfinity;
                }
                else
                {
                    _lastTapTime = frame.Time;
                    _lastTapPosition = pointer.Position;
                }
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            _lastTapTime = float.NegativeInfinity;
        }
    }
}
