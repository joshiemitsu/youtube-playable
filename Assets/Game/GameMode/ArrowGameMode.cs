using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using RapidPro.Core;
using RapidPro.Display;
using RapidPro.GameLoop;
using RapidPro.Input;
using RapidPro.Pooling;
using UnityEngine;

/// <summary>
/// The arrow game's rules, plugged into the framework's game loop.
/// <list type="bullet">
/// <item>Tap an arrow: if its path is clear it leaves the board, otherwise it bumps and costs a heart.</item>
/// <item>All arrows gone: win. No hearts left: lose.</item>
/// </list>
/// Lives on the ArrowGame prefab (a variant of the framework's GameRoot) next to its GameLoopRunner.
/// </summary>
public class ArrowGameMode : MonoBehaviour, IGameMode
{
    [Header("Board")]
    [SerializeField] private GameBoardSystem _board;
    [SerializeField] private BoardInitializer _builder;

    [Header("Tapping")]
    [Tooltip("Layer of the arrow head and body colliders.")]
    [SerializeField] private LayerMask _arrowLayer = 1 << 6;

    [Header("Rules")]
    [Tooltip("Tries per level. Each arrow that bumps into another costs one.")]
    [Range(1, 10)]
    [SerializeField] private int _maxHearts = 3;

    private HeartsModel _hearts;
    private IGameLoop _loop;
    private IDisposable _tapSubscription;

    private void Awake()
    {
        _hearts = new HeartsModel(_maxHearts);

        // The HUD in the GameUI scene reads the hearts from here
        if (Services.TryGet(out IHearts _)) Services.Unregister<IHearts>();
        Services.Register<IHearts>(_hearts);
    }

    private void OnDestroy()
    {
        _tapSubscription?.Dispose();

        if (Services.TryGet(out IHearts registered) && registered == _hearts) Services.Unregister<IHearts>();
        _hearts.Dispose();
    }

    public UniTask PrepareAsync(LevelData p_level, CancellationToken p_cancellationToken)
    {
        if (p_level is not ArrowLevelData arrowLevel)
        {
            throw new ArgumentException($"ArrowGameMode needs an ArrowLevelData, but got {p_level.GetType().Name}.");
        }

        _builder.Build(arrowLevel, Services.Get<IPoolService>());
        _hearts.Reset(_maxHearts);
        FitCamera();

        return UniTask.CompletedTask;
    }

    public void Begin()
    {
        _loop = Services.Get<IGameLoop>();

        // One arrow at a time: taps while an arrow is moving are dropped
        _tapSubscription = Services.Get<IInputService>().Tapped
            .Select(tap => FindArrow(tap.Position))
            .Where(arrow => arrow != null)
            .SubscribeAwait(async (arrow, ct) => await HandleTap(arrow), AwaitOperation.Drop);
    }

    public void Cleanup()
    {
        _tapSubscription?.Dispose();
        _tapSubscription = null;
        _builder.Clear();
    }

    private async UniTask HandleTap(ArrowEntry p_arrow)
    {
        if (_loop.State.CurrentValue != GameState.Playing)
        {
            return;
        }

        ArrowTapResult result = p_arrow.PreviewTap();
        if (result == ArrowTapResult.Ignored)
        {
            return;
        }

        // Take the heart straight away so the HUD reacts on the hit, not after the bounce back
        bool isOutOfHearts = result == ArrowTapResult.Blocked && _hearts.LoseOne();

        await p_arrow.OnPressed();

        if (result == ArrowTapResult.Exited)
        {
            _builder.Despawn(p_arrow);
            if (_builder.Arrows.Count == 0)
            {
                _loop.Win();
            }
        }
        else if (isOutOfHearts)
        {
            _loop.Lose();
        }
    }

    private ArrowEntry FindArrow(Vector2 p_screenPosition)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return null;
        }

        Vector3 screenPoint = new Vector3(p_screenPosition.x, p_screenPosition.y, -camera.transform.position.z);
        Vector2 worldPos = camera.ScreenToWorldPoint(screenPoint);

        Collider2D hit = Physics2D.OverlapPoint(worldPos, _arrowLayer);
        if (hit != null && hit.TryGetComponent(out ArrowSubUnit subUnit) && subUnit.Parent != null)
        {
            return subUnit.Parent;
        }
        return null;
    }

    private void FitCamera()
    {
        Camera camera = Camera.main;
        if (camera != null && camera.TryGetComponent(out CameraFitter fitter))
        {
            fitter.SetBounds(_board.GetPlayableWorldBounds());
        }
    }
}
