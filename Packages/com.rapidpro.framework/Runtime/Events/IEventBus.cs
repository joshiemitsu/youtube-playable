using R3;

namespace RapidPro.Events
{
    /// <summary>
    /// Sends events between features without them knowing about each other,
    /// e.g. gameplay tells UI, audio and save that a level was completed.
    /// </summary>
    /// <remarks>
    /// Use it <b>between</b> features. Inside one feature, bind directly instead.
    /// Events are small structs named in the past tense.
    /// </remarks>
    /// <example>
    /// <code>
    /// public readonly struct LevelCompleted
    /// {
    ///     public readonly int LevelIndex;
    ///     public LevelCompleted(int levelIndex) => LevelIndex = levelIndex;
    /// }
    ///
    /// // Gameplay
    /// _events.Publish(new LevelCompleted(3));
    ///
    /// // Audio, in a MonoBehaviour
    /// _events.On&lt;LevelCompleted&gt;()
    ///     .Subscribe(e => _audio.PlaySfx(_winCue))
    ///     .AddTo(this);
    /// </code>
    /// </example>
    public interface IEventBus
    {
        /// <summary>
        /// Sends <paramref name="evt"/> to everyone currently listening for <typeparamref name="T"/>.
        /// Does nothing when no one is listening.
        /// </summary>
        /// <param name="evt">The event to send.</param>
        void Publish<T>(T evt) where T : struct;

        /// <summary>
        /// Returns a stream of every <typeparamref name="T"/> event published from now on.
        /// Always end the subscription with <c>.AddTo(...)</c>.
        /// </summary>
        Observable<T> On<T>() where T : struct;
    }
}
