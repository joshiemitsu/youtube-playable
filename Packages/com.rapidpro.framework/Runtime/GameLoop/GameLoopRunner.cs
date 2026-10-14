using Cysharp.Threading.Tasks;
using RapidPro.Core;
using RapidPro.Events;
using RapidPro.Save;
using UnityEngine;

namespace RapidPro.GameLoop
{
    /// <summary>
    /// Connects a game to the framework. It lives on the root of the framework's <b>GameRoot</b> prefab.
    /// A game makes a prefab variant of GameRoot, adds its <see cref="IGameMode"/> component next to this
    /// one, assigns its <see cref="LevelCatalog"/>, and drops the variant into its Game scene.
    /// </summary>
    /// <remarks>
    /// On Awake it creates the <see cref="GameLoop"/> and registers it as <see cref="IGameLoop"/>, so UI in
    /// the GameUI scene can find it. On Start it plays the saved level. Save and the event bus are used
    /// when the Bootstrap scene registered them.
    /// </remarks>
    [DisallowMultipleComponent]
    public class GameLoopRunner : MonoBehaviour
    {
        [Tooltip("The game's levels in play order.")]
        [SerializeField] private LevelCatalog _levels;

        [Tooltip("Start the saved level as soon as the scene starts. Untick to start it from code with IGameLoop.StartAsync.")]
        [SerializeField] private bool _startOnPlay = true;

        private GameLoop _loop;

        /// <summary>The loop this runner created, or null if setup failed.</summary>
        public IGameLoop Loop => _loop;

        private void Awake()
        {
            var gameMode = GetComponent<IGameMode>();
            if (gameMode == null)
            {
                Log.Error("GameLoopRunner: add your game's IGameMode component to the same GameObject.", this);
                return;
            }

            if (_levels == null)
            {
                Log.Error("GameLoopRunner: assign a Level Catalog.", this);
                return;
            }

            Services.TryGet(out ISaveService save);
            Services.TryGet(out IEventBus events);
            _loop = new GameLoop(gameMode, _levels, save, events, destroyCancellationToken);

            if (Services.TryGet(out IGameLoop _))
                Services.Unregister<IGameLoop>();
            Services.Register<IGameLoop>(_loop);
        }

        private void Start()
        {
            if (_loop != null && _startOnPlay)
                _loop.StartAsync().Forget();
        }

        private void OnDestroy()
        {
            if (_loop == null)
                return;

            if (Services.TryGet(out IGameLoop registered) && registered == _loop)
                Services.Unregister<IGameLoop>();

            _loop.Dispose();
        }
    }
}
