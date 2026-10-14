namespace RapidPro.GameLoop
{
    /// <summary>Published on the event bus when a level is won.</summary>
    public readonly struct LevelCompleted
    {
        /// <summary>Index of the level, starting at 0.</summary>
        public readonly int LevelIndex;

        /// <summary>Creates the event.</summary>
        public LevelCompleted(int levelIndex) => LevelIndex = levelIndex;
    }
}
