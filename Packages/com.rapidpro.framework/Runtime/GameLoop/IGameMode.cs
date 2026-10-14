using System.Threading;
using Cysharp.Threading.Tasks;

namespace RapidPro.GameLoop
{
    /// <summary>
    /// The contract a game implements to plug into the framework's game loop. The loop decides
    /// <i>when</i>; the game mode decides <i>what</i>. The game reports the result by calling
    /// <see cref="IGameLoop.Win"/> or <see cref="IGameLoop.Lose"/>.
    /// </summary>
    /// <remarks>
    /// Put the game mode on the same GameObject as a <see cref="GameLoopRunner"/> (a variant of the
    /// framework's GameRoot prefab). The loop calls, in order:
    /// <list type="number">
    /// <item><see cref="Cleanup"/>, so a restart starts from a clean board.</item>
    /// <item><see cref="PrepareAsync"/>, to build the level.</item>
    /// <item><see cref="Begin"/>, when the player can act.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyGameMode : MonoBehaviour, IGameMode
    /// {
    ///     public async UniTask PrepareAsync(LevelData level, CancellationToken ct) { /* spawn the board */ }
    ///     public void Begin() { /* listen to input */ }
    ///     public void Cleanup() { /* despawn everything */ }
    ///
    ///     private void OnGoalReached() => Services.Get&lt;IGameLoop&gt;().Win();
    /// }
    /// </code>
    /// </example>
    public interface IGameMode
    {
        /// <summary>Builds <paramref name="level"/>: board, pieces, camera. Input should not act yet.</summary>
        UniTask PrepareAsync(LevelData level, CancellationToken cancellationToken);

        /// <summary>Lets the player play.</summary>
        void Begin();

        /// <summary>Removes everything the level spawned. Must be safe to call when nothing was spawned.</summary>
        void Cleanup();
    }
}
