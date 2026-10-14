using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace RapidPro.Scenes
{
    /// <summary>
    /// Starts the game from the Bootstrap scene: shows Loading, opens Game and GameUI, then hides Loading.
    /// </summary>
    /// <example>
    /// <code>
    /// // A loading bar
    /// Services.Get&lt;ISceneFlow&gt;().Progress.Subscribe(p => _fill.fillAmount = p).AddTo(this);
    /// </code>
    /// </example>
    public interface ISceneFlow
    {
        /// <summary>Loading progress, 0 to 1.</summary>
        ReadOnlyReactiveProperty<float> Progress { get; }

        /// <summary>True while scenes are loading.</summary>
        ReadOnlyReactiveProperty<bool> IsLoading { get; }

        /// <summary>
        /// Loads the game. Normally opens the Game and GameUI scenes. When <paramref name="startScene"/>
        /// is given (e.g. a sandbox scene opened with "play from any scene"), opens only that scene instead.
        /// Ignored, with a warning, while a load is already running.
        /// </summary>
        UniTask StartGameAsync(string startScene = null, CancellationToken cancellationToken = default);
    }
}
