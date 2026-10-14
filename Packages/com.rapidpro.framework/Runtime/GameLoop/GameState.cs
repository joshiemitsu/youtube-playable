namespace RapidPro.GameLoop
{
    /// <summary>Where the game loop is: <c>Home → Playing → Win or Lose → Playing</c>.</summary>
    public enum GameState
    {
        /// <summary>Before the first level starts. A "tap to play" screen can live here later.</summary>
        Home,

        /// <summary>A level is being prepared or played.</summary>
        Playing,

        /// <summary>The level was won.</summary>
        Win,

        /// <summary>The level was lost.</summary>
        Lose
    }
}
