namespace RapidPro.Input
{
    /// <summary>
    /// Turns pointer frames into one kind of gesture. Every built-in gesture is one of these, and you add
    /// your own the same way with <see cref="IInputService.AddRecognizer"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// // A two-finger tap, as a custom gesture
    /// public class TwoFingerTapRecognizer : IGestureRecognizer
    /// {
    ///     private readonly Subject&lt;Unit&gt; _tapped = new Subject&lt;Unit&gt;();
    ///     public Observable&lt;Unit&gt; Tapped => _tapped;
    ///
    ///     public void Process(PointerFrame frame, InputConfig config)
    ///     {
    ///         if (frame.DownCount == 0 &amp;&amp; frame.Pointers.Count == 2 &amp;&amp; frame.Pointers[0].Phase == PointerPhase.Ended)
    ///             _tapped.OnNext(Unit.Default);
    ///     }
    ///
    ///     public void Reset() { }
    /// }
    ///
    /// var twoFingerTap = new TwoFingerTapRecognizer();
    /// Services.Get&lt;IInputService&gt;().AddRecognizer(twoFingerTap);
    /// twoFingerTap.Tapped.Subscribe(_ => ShowHint()).AddTo(this);
    /// </code>
    /// </example>
    public interface IGestureRecognizer
    {
        /// <summary>Reads one frame of pointers and fires events when the gesture happens.</summary>
        /// <param name="frame">The pointers this frame, with their history.</param>
        /// <param name="config">Thresholds from the input config.</param>
        void Process(PointerFrame frame, InputConfig config);

        /// <summary>Forgets any gesture in progress, e.g. when input is blocked.</summary>
        void Reset();
    }
}
