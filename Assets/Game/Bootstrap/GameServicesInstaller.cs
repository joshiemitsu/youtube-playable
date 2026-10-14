using RapidPro.Core;
using RapidPro.Events;
using RapidPro.Input;
using RapidPro.Lifecycle;
using RapidPro.Pooling;
using RapidPro.Save;
using RapidPro.Scenes;
using UnityEngine;

/// <summary>
/// Registers every framework service the game uses. Lives in the Bootstrap scene, which never unloads.
/// </summary>
public class GameServicesInstaller : GameInstaller
{
    [Header("Scenes")]
    [SerializeField] private SceneFlowSettings _sceneFlowSettings;

    [Header("Components in this scene")]
    [SerializeField] private InputService _input;
    [SerializeField] private AppLifecycle _lifecycle;

    [Header("Pooling")]
    [Tooltip("Optional. Prefabs to create up front.")]
    [SerializeField] private PoolConfig _poolConfig;

    protected override void Install()
    {
        // Static services can survive between Play sessions when domain reload is off, so start clean
        Services.Clear();

        var save = new SaveService();
        var pause = new PauseService();

        Services.Register<ISaveService>(save);
        Services.Register<IEventBus>(new EventBus());
        Services.Register<IPauseService>(pause);
        Services.Register<IPoolService>(new PoolService(_poolConfig, transform));
        Services.Register<IInputService>(_input);

        _lifecycle.Connect(pause, save);
        Services.Register<IAppLifecycle>(_lifecycle);

        Services.Register<ISceneFlow>(new SceneFlow(_sceneFlowSettings));
    }

    private void OnDestroy()
    {
        Services.Clear();
    }
}
