using R3;

namespace RapidPro.Input
{
    /// <summary>
    /// Touch, mouse and scroll-wheel gestures as event streams. Gameplay subscribes to the gestures it
    /// needs; screens and popups block input while they are open.
    /// </summary>
    /// <example>
    /// <code>
    /// var input = Services.Get&lt;IInputService&gt;();
    ///
    /// input.Tapped.Subscribe(tap => SelectAt(tap.Position)).AddTo(this);
    /// input.Flicked.Subscribe(flick => Move(flick.Direction)).AddTo(this);
    /// input.Pinched.Subscribe(pinch => Zoom(pinch.Delta, pinch.Center)).AddTo(this);
    ///
    /// input.Block("popup");    // while a popup is open
    /// input.Unblock("popup");
    /// </code>
    /// </example>
    public interface IInputService
    {
        /// <summary>A finger went down.</summary>
        Observable<PressEvent> Pressed { get; }

        /// <summary>A finger was released (tap release), whatever gesture it was.</summary>
        Observable<PressEvent> Released { get; }

        /// <summary>A quick touch and release without moving.</summary>
        Observable<TapEvent> Tapped { get; }

        /// <summary>Two quick taps close together.</summary>
        Observable<TapEvent> DoubleTapped { get; }

        /// <summary>A finger held still for the hold time.</summary>
        Observable<HoldEvent> Held { get; }

        /// <summary>A finger started dragging.</summary>
        Observable<DragEvent> DragStarted { get; }

        /// <summary>A dragging finger moved.</summary>
        Observable<DragEvent> Dragged { get; }

        /// <summary>A drag finished.</summary>
        Observable<DragEvent> DragEnded { get; }

        /// <summary>A quick swipe released while moving fast.</summary>
        Observable<FlickEvent> Flicked { get; }

        /// <summary>Two fingers (or the scroll wheel) zooming, in or out.</summary>
        Observable<PinchEvent> Pinched { get; }

        /// <summary>Fingers moving together.</summary>
        Observable<PinchEvent> PinchedIn { get; }

        /// <summary>Fingers moving apart.</summary>
        Observable<PinchEvent> PinchedOut { get; }

        /// <summary>True while input is blocked for at least one reason.</summary>
        ReadOnlyReactiveProperty<bool> IsBlocked { get; }

        /// <summary>
        /// Stops all gestures for <paramref name="reason"/>, e.g. while a popup or transition shows.
        /// Gestures in progress are dropped. Fingers still down when input unblocks are ignored until lifted.
        /// </summary>
        void Block(string reason);

        /// <summary>Removes <paramref name="reason"/>. Input works again when no reasons are left.</summary>
        void Unblock(string reason);

        /// <summary>Adds a custom gesture. It receives every pointer frame after the built-in gestures.</summary>
        void AddRecognizer(IGestureRecognizer recognizer);

        /// <summary>Removes a custom gesture added with <see cref="AddRecognizer"/>.</summary>
        void RemoveRecognizer(IGestureRecognizer recognizer);
    }
}
