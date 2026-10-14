using Cysharp.Threading.Tasks;
using RapidPro.Core;
using UnityEngine;

namespace RapidPro.Scenes
{
    /// <summary>
    /// Starts the game once Bootstrap's services are registered. Put it in the Bootstrap scene next to
    /// your <c>GameInstaller</c>. The installer registers in <c>Awake</c>; this starts in <c>Start</c>,
    /// which Unity always runs after every <c>Awake</c> in the scene.
    /// </summary>
    public class SceneFlowStarter : MonoBehaviour
    {
        private void Start()
        {
            if (!Services.TryGet(out ISceneFlow flow))
            {
                Log.Error("SceneFlowStarter: no ISceneFlow is registered. Register a SceneFlow in your GameInstaller.", this);
                return;
            }

            flow.StartGameAsync(PlayFromAnyScene.ConsumeRequestedScene(), destroyCancellationToken).Forget();
        }
    }
}
