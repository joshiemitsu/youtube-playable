namespace RapidPro.GameLoop
{
    /// <summary>Published on the event bus when a level is lost.</summary>
    public readonly struct LevelFailed
    {
        /// <summary>Index of the level, starting at 0.</summary>
        public readonly int LevelIndex;

        /// <summary>Creates the event.</summary>
        public LevelFailed(int levelIndex) => LevelIndex = levelIndex;
    }
}
