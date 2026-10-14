using R3;

namespace RapidPro.Lifecycle
{
    /// <summary>
    /// Pauses the game for one or more reasons. The game stays paused until every reason is resumed,
    /// so closing a popup does not unpause a game that the player also paused.
    /// </summary>
    /// <example>
    /// <code>
    /// _pause.Pause(PauseReasons.Popup);
    /// // ... popup closes
    /// _pause.Resume(PauseReasons.Popup);
    ///
    /// _pause.IsPaused.Subscribe(paused => _pauseOverlay.SetActive(paused)).AddTo(this);
    /// </code>
    /// </example>
    public interface IPauseService
    {
        /// <summary>True while at least one reason is paused.</summary>
        ReadOnlyReactiveProperty<bool> IsPaused { get; }

        /// <summary>
        /// Pauses for <paramref name="reason"/>. Pausing again for the same reason changes nothing,
        /// so one <see cref="Resume"/> is always enough to undo it.
        /// </summary>
        void Pause(string reason);

        /// <summary>Removes <paramref name="reason"/>. The game resumes when no reasons are left.</summary>
        void Resume(string reason);

        /// <summary>True while the game is paused for <paramref name="reason"/>.</summary>
        bool IsPausedFor(string reason);
    }
}
