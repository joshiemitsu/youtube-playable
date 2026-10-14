using System.Threading;
using Cysharp.Threading.Tasks;

namespace RapidPro.StateMachine
{
    /// <summary>
    /// Base class for one state in a <see cref="StateMachine{TStateId}"/>.
    /// Override only what you need; every method does nothing by default.
    /// </summary>
    /// <remarks>
    /// For an instant enter, override <see cref="Enter"/>.
    /// For an animated enter (e.g. wait for a win screen to slide in), override <see cref="EnterAsync"/> instead.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class PlayingState : State
    /// {
    ///     private readonly IEventBus _events;
    ///     public PlayingState(IEventBus events) => _events = events;
    ///
    ///     protected override void Enter() => _events.Publish(new LevelStarted());
    ///     public override void Tick(float deltaTime) { /* update the level timer */ }
    /// }
    ///
    /// public class WinState : State
    /// {
    ///     public override async UniTask EnterAsync(CancellationToken cancellationToken)
    ///     {
    ///         await _winScreen.PlayShowAnimation(cancellationToken);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class State
    {
        /// <summary>
        /// Called when the state machine changes to this state. The machine waits for it to finish.
        /// By default it calls <see cref="Enter"/> and completes at once.
        /// </summary>
        /// <param name="cancellationToken">Cancelled when the owner is destroyed.</param>
        public virtual UniTask EnterAsync(CancellationToken cancellationToken)
        {
            Enter();
            return UniTask.CompletedTask;
        }

        /// <summary>Called every frame while this is the current state.</summary>
        /// <param name="deltaTime">Seconds since the last tick, usually <c>Time.deltaTime</c>.</param>
        public virtual void Tick(float deltaTime) { }

        /// <summary>Called when the state machine leaves this state.</summary>
        public virtual void Exit() { }

        /// <summary>Instant enter. Override this unless you need to wait for something.</summary>
        protected virtual void Enter() { }
    }
}
