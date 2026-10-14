namespace RapidPro.Lifecycle
{
    /// <summary>
    /// Common reasons to pass to <see cref="IPauseService.Pause"/> and <see cref="IPauseService.Resume"/>.
    /// Games can use their own strings too.
    /// </summary>
    public static class PauseReasons
    {
        /// <summary>The app is in the background. Used by <see cref="AppLifecycle"/>.</summary>
        public const string Background = "background";

        /// <summary>A popup is open.</summary>
        public const string Popup = "popup";

        /// <summary>The debug panel is open.</summary>
        public const string Debug = "debug";

        /// <summary>The player pressed the pause button.</summary>
        public const string Player = "player";
    }
}
