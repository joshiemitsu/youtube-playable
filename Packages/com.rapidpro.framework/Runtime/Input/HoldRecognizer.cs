using System.Collections.Generic;
using R3;

namespace RapidPro.Input
{
    /// <summary>
    /// Fires <see cref="Held"/> once when a finger stays down, without moving, for the hold time.
    /// A held finger does not also count as a tap when released.
    /// </summary>
    public class HoldRecognizer : IGestureRecognizer
    {
        private readonly Subject<HoldEvent> _held = new Subject<HoldEvent>();
        private readonly HashSet<int> _alreadyHeld = new HashSet<int>();

        /// <summary>A finger was held still for the hold time.</summary>
        public Observable<HoldEvent> Held => _held;

        /// <inheritdoc />
        public void Process(PointerFrame frame, InputConfig config)
        {
            foreach (var pointer in frame.Pointers)
            {
                if (pointer.Phase == PointerPhase.Ended)
                {
                    _alreadyHeld.Remove(pointer.Id);
                    continue;
                }

                if (!PointerFilter.IsSingleFinger(pointer, config) || _alreadyHeld.Contains(pointer.Id))
                    continue;

                float duration = frame.Time - pointer.StartTime;
                bool still = pointer.MaxDistance <= frame.ToPixels(config.DragThreshold);

                if (still && duration >= config.HoldTime)
                {
                    _alreadyHeld.Add(pointer.Id);
                    _held.OnNext(new HoldEvent(pointer.Position, duration));
                }
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            _alreadyHeld.Clear();
        }
    }
}
