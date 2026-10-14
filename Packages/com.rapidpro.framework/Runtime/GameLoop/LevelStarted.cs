namespace RapidPro.GameLoop
{
    /// <summary>Published on the event bus when a level starts. Useful for analytics and audio.</summary>
    public readonly struct LevelStarted
    {
        /// <summary>Index of the level, starting at 0.</summary>
        public readonly int LevelIndex;

        /// <summary>Creates the event.</summary>
        public LevelStarted(int levelIndex) => LevelIndex = levelIndex;
    }
}
