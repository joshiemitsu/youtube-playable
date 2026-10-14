using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using RapidPro.Core;

namespace RapidPro.StateMachine
{
    /// <summary>
    /// A simple state machine keyed by an enum, such as the game loop
    /// <c>Home → Playing → Win or Lose → Home</c>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A change calls <see cref="State.Exit"/> on the old state, updates <see cref="CurrentState"/>, then waits for <see cref="State.EnterAsync"/> on the new one.</item>
    /// <item>A change requested while another change is still running is <b>ignored</b> with a warning. It is not queued.</item>
    /// <item>Changing to the state you are already in exits and re-enters it, which is handy for Retry.</item>
    /// <item><see cref="Tick"/> does nothing while a change is running.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public enum GameState { Home, Playing, Win, Lose }
    ///
    /// _loop = new StateMachine&lt;GameState&gt;();
    /// _loop.Register(GameState.Home, new HomeState());
    /// _loop.Register(GameState.Playing, new PlayingState(events));
    /// _loop.Register(GameState.Win, new WinState());
    /// _loop.Register(GameState.Lose, new LoseState());
    ///
    /// _loop.ChangeStateAsync(GameState.Home, destroyCancellationToken).Forget();
    ///
    /// // UI reacts to the current state
    /// _loop.CurrentState.Subscribe(state => _pauseButton.SetActive(state == GameState.Playing)).AddTo(this);
    ///
    /// // In a MonoBehaviour's Update
    /// _loop.Tick(Time.deltaTime);
    /// </code>
    /// </example>
    /// <typeparam name="TStateId">An enum that names the states.</typeparam>
    public class StateMachine<TStateId> : IDisposable where TStateId : struct, Enum
    {
        private readonly Dictionary<TStateId, State> _states = new Dictionary<TStateId, State>();
        private readonly ReactiveProperty<TStateId> _currentState = new ReactiveProperty<TStateId>();
        private State _current;
        private bool _isChanging;

        /// <summary>
        /// The current state id. Emits on every change, including re-entering the same state.
        /// Holds <c>default(TStateId)</c> until the first change; check <see cref="HasState"/> if that matters.
        /// </summary>
        public ReadOnlyReactiveProperty<TStateId> CurrentState => _currentState;

        /// <summary>True after the first change has started.</summary>
        public bool HasState => _current != null;

        /// <summary>True while a change is running, e.g. during an animated enter.</summary>
        public bool IsChanging => _isChanging;

        /// <summary>
        /// Adds a state under <paramref name="id"/>. Register every state before the first change.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="id"/> is already registered.</exception>
        public void Register(TStateId id, State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state), $"Cannot register a null state for {id}.");

            if (_states.ContainsKey(id))
                throw new InvalidOperationException($"State {id} is already registered.");

            _states[id] = state;
        }

        /// <summary>
        /// Exits the current state and enters the state registered under <paramref name="id"/>.
        /// Ignored, with a warning, if another change is still running.
        /// </summary>
        /// <param name="id">The state to change to.</param>
        /// <param name="cancellationToken">Passed to <see cref="State.EnterAsync"/>.</param>
        /// <returns>Completes when the new state has finished entering.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> was never registered.</exception>
        public UniTask ChangeStateAsync(TStateId id, CancellationToken cancellationToken = default)
        {
            if (!_states.TryGetValue(id, out var next))
                throw new ArgumentException($"State {id} is not registered. Call Register first.", nameof(id));

            if (_isChanging)
            {
                Log.Warning($"StateMachine: ignored change to {id} because a change to {_currentState.CurrentValue} is still running.");
                return UniTask.CompletedTask;
            }

            return RunChange(id, next, cancellationToken);
        }

        /// <summary>
        /// Ticks the current state. Call it from a MonoBehaviour's <c>Update</c>.
        /// Does nothing before the first change or while a change is running.
        /// </summary>
        /// <param name="deltaTime">Seconds since the last tick, usually <c>Time.deltaTime</c>.</param>
        public void Tick(float deltaTime)
        {
            if (_isChanging)
                return;

            _current?.Tick(deltaTime);
        }

        /// <summary>
        /// Completes <see cref="CurrentState"/>. Does not call <see cref="State.Exit"/>.
        /// </summary>
        public void Dispose()
        {
            _currentState.Dispose();
        }

        private async UniTask RunChange(TStateId id, State next, CancellationToken cancellationToken)
        {
            _isChanging = true;
            try
            {
                _current?.Exit();
                _current = next;
                _currentState.OnNext(id);
                await next.EnterAsync(cancellationToken);
            }
            finally
            {
                _isChanging = false;
            }
        }
    }
}
