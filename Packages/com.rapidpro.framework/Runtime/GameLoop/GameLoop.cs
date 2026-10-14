using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using RapidPro.Core;
using RapidPro.Events;
using RapidPro.Save;
using RapidPro.StateMachine;

namespace RapidPro.GameLoop
{
    /// <summary>
    /// Default <see cref="IGameLoop"/>, built on <see cref="StateMachine{TStateId}"/>.
    /// Normally created by a <see cref="GameLoopRunner"/>; create it yourself only in tests or tools.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><b>Playing</b>: cleans up, prepares the level, then begins it. Publishes <see cref="LevelStarted"/>.</item>
    /// <item><b>Win</b>: saves the next level index and publishes <see cref="LevelCompleted"/>.</item>
    /// <item><b>Lose</b>: publishes <see cref="LevelFailed"/>.</item>
    /// </list>
    /// Win and Lose are ignored unless a level is being played, so a late or repeated call is harmless.
    /// </remarks>
    public class GameLoop : IGameLoop, IDisposable
    {
        /// <summary>Save key holding the index of the level to play next.</summary>
        public const string LevelSaveKey = "progress.level";

        private readonly IGameMode _gameMode;
        private readonly LevelCatalog _levels;
        private readonly ISaveService _save;
        private readonly IEventBus _events;
        private readonly CancellationToken _lifetime;
        private readonly StateMachine<GameState> _machine = new StateMachine<GameState>();
        private readonly ReactiveProperty<int> _levelIndex = new ReactiveProperty<int>(0);

        /// <summary>Creates the loop. Save and events are optional.</summary>
        /// <param name="gameMode">The game that builds and plays levels.</param>
        /// <param name="levels">The levels in play order.</param>
        /// <param name="save">Remembers the level to continue from. Optional.</param>
        /// <param name="events">Receives level started/completed/failed events. Optional.</param>
        /// <param name="lifetime">Cancels a level that is still preparing when the owner is destroyed.</param>
        public GameLoop(IGameMode gameMode, LevelCatalog levels, ISaveService save = null, IEventBus events = null,
            CancellationToken lifetime = default)
        {
            _gameMode = gameMode ?? throw new ArgumentNullException(nameof(gameMode));
            _levels = levels != null ? levels : throw new ArgumentNullException(nameof(levels));
            _save = save;
            _events = events;
            _lifetime = lifetime;

            _machine.Register(GameState.Home, new HomeState());
            _machine.Register(GameState.Playing, new PlayingState(this));
            _machine.Register(GameState.Win, new WinState(this));
            _machine.Register(GameState.Lose, new LoseState(this));
        }

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<GameState> State => _machine.CurrentState;

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<int> LevelIndex => _levelIndex;

        /// <summary>The level being played.</summary>
        public LevelData CurrentLevel => _levels.Get(_levelIndex.Value);

        /// <inheritdoc />
        public UniTask StartAsync()
        {
            _levelIndex.Value = _save?.Load(LevelSaveKey, 0) ?? 0;
            return _machine.ChangeStateAsync(GameState.Playing, _lifetime);
        }

        /// <inheritdoc />
        public void Win() => Finish(GameState.Win);

        /// <inheritdoc />
        public void Lose() => Finish(GameState.Lose);

        /// <inheritdoc />
        public UniTask RestartAsync()
        {
            return _machine.ChangeStateAsync(GameState.Playing, _lifetime);
        }

        /// <inheritdoc />
        public UniTask NextLevelAsync()
        {
            if (_machine.IsChanging)
                return UniTask.CompletedTask;

            _levelIndex.Value++;
            return _machine.ChangeStateAsync(GameState.Playing, _lifetime);
        }

        /// <summary>Completes the observable state and level index.</summary>
        public void Dispose()
        {
            _machine.Dispose();
            _levelIndex.Dispose();
        }

        private void Finish(GameState result)
        {
            if (State.CurrentValue != GameState.Playing || _machine.IsChanging)
            {
                Log.Warning($"GameLoop: ignored {result} because no level is being played (state: {State.CurrentValue}).");
                return;
            }

            _machine.ChangeStateAsync(result, _lifetime).Forget();
        }

        private class HomeState : State { }

        private class PlayingState : State
        {
            private readonly GameLoop _loop;

            public PlayingState(GameLoop loop) => _loop = loop;

            public override async UniTask EnterAsync(CancellationToken cancellationToken)
            {
                _loop._gameMode.Cleanup();
                await _loop._gameMode.PrepareAsync(_loop.CurrentLevel, cancellationToken);
                _loop._gameMode.Begin();
                _loop._events?.Publish(new LevelStarted(_loop._levelIndex.Value));
            }
        }

        private class WinState : State
        {
            private readonly GameLoop _loop;

            public WinState(GameLoop loop) => _loop = loop;

            protected override void Enter()
            {
                if (_loop._save != null)
                {
                    _loop._save.Save(LevelSaveKey, _loop._levelIndex.Value + 1);
                    _loop._save.Flush();
                }

                _loop._events?.Publish(new LevelCompleted(_loop._levelIndex.Value));
            }
        }

        private class LoseState : State
        {
            private readonly GameLoop _loop;

            public LoseState(GameLoop loop) => _loop = loop;

            protected override void Enter()
            {
                _loop._events?.Publish(new LevelFailed(_loop._levelIndex.Value));
            }
        }
    }
}
