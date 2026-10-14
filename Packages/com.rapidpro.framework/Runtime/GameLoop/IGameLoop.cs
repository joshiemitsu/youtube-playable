using Cysharp.Threading.Tasks;
using R3;

namespace RapidPro.GameLoop
{
    /// <summary>
    /// Runs levels: <c>Playing → Win or Lose</c>, restart and next level. The game calls
    /// <see cref="Win"/> or <see cref="Lose"/>; UI listens to <see cref="State"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// var loop = Services.Get&lt;IGameLoop&gt;();
    /// loop.State.Subscribe(state => _winScreen.SetActive(state == GameState.Win)).AddTo(this);
    /// _retryButton.onClick.AddListener(() => loop.RestartAsync().Forget());
    /// </code>
    /// </example>
    public interface IGameLoop
    {
        /// <summary>The current state. UI shows and hides screens from this.</summary>
        ReadOnlyReactiveProperty<GameState> State { get; }

        /// <summary>Index of the level being played, starting at 0. Show <c>LevelIndex + 1</c> to players.</summary>
        ReadOnlyReactiveProperty<int> LevelIndex { get; }

        /// <summary>Starts the saved level (or the first one).</summary>
        UniTask StartAsync();

        /// <summary>The level was won. Ignored unless a level is being played.</summary>
        void Win();

        /// <summary>The level was lost. Ignored unless a level is being played.</summary>
        void Lose();

        /// <summary>Plays the current level again from the start.</summary>
        UniTask RestartAsync();

        /// <summary>Moves on to the next level, looping after the last.</summary>
        UniTask NextLevelAsync();
    }
}
