using R3;

namespace RapidPro.Lifecycle
{
    /// <summary>
    /// Tells the game when the app goes to the background, comes back, or the Back button is pressed.
    /// </summary>
    /// <example>
    /// <code>
    /// _lifecycle.ReturnedFromBackground.Subscribe(_ => _tapToContinue.Show()).AddTo(this);
    /// _lifecycle.BackPressed.Subscribe(_ => _screens.CloseTopOrAskToQuit()).AddTo(this);
    /// </code>
    /// </example>
    public interface IAppLifecycle
    {
        /// <summary>Fires after the game has paused and saved, as the app goes to the background.</summary>
        Observable<Unit> WentToBackground { get; }

        /// <summary>Fires when the app comes back to the foreground.</summary>
        Observable<Unit> ReturnedFromBackground { get; }

        /// <summary>Fires when the Android Back button (Escape on desktop) is pressed.</summary>
        Observable<Unit> BackPressed { get; }
    }
}
