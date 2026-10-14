using R3;

namespace RapidPro.Input
{
    /// <summary>
    /// Fires when any finger goes down (<see cref="Pressed"/>) or is released (<see cref="Released"/>),
    /// whatever gesture it turns into. Use Released for "on tap release" behaviour.
    /// </summary>
    public class PressRecognizer : IGestureRecognizer
    {
        private readonly Subject<PressEvent> _pressed = new Subject<PressEvent>();
        private readonly Subject<PressEvent> _released = new Subject<PressEvent>();

        /// <summary>A finger went down.</summary>
        public Observable<PressEvent> Pressed => _pressed;

        /// <summary>A finger was lifted. Tells you how long it was down and whether it dragged.</summary>
        public Observable<PressEvent> Released => _released;

        /// <inheritdoc />
        public void Process(PointerFrame frame, InputConfig config)
        {
            foreach (var pointer in frame.Pointers)
            {
                if (PointerFilter.IsIgnored(pointer, config))
                    continue;

                if (pointer.Phase == PointerPhase.Began)
                {
                    _pressed.OnNext(new PressEvent(pointer.Position, 0f, false));
                }
                else if (pointer.Phase == PointerPhase.Ended)
                {
                    bool wasDrag = pointer.MaxDistance > frame.ToPixels(config.DragThreshold);
                    _released.OnNext(new PressEvent(pointer.Position, frame.Time - pointer.StartTime, wasDrag));
                }
            }
        }

        /// <inheritdoc />
        public void Reset() { }
    }
}
