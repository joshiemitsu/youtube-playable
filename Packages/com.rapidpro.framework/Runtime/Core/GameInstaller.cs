using UnityEngine;

namespace RapidPro.Core
{
    /// <summary>
    /// Base class for the scene component that registers your game's services.
    /// Subclass it, register every service by hand in <see cref="Install"/>, and put it in your first scene.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyGameInstaller : GameInstaller
    /// {
    ///     protected override void Install()
    ///     {
    ///         Services.Register&lt;IScoreService&gt;(new ScoreService());
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class GameInstaller : MonoBehaviour
    {
        /// <summary>
        /// Calls <see cref="Install"/>. If you override this, call <c>base.Awake()</c> first.
        /// </summary>
        protected virtual void Awake()
        {
            Install();
        }

        /// <summary>
        /// Registers the game's services with <see cref="Services.Register{T}"/>. Called once in <see cref="Awake"/>.
        /// </summary>
        protected abstract void Install();
    }
}
