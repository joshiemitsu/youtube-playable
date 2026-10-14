# RapidPro Framework

A small, readable framework for rapid prototyping hyper/hybrid-casual games in Unity.

## Modules

| Module | Assembly | What it gives you |
|---|---|---|
| Core | `RapidPro.Core` | `Services` (service locator), `Log`, `GameInstaller` |
| Events | `RapidPro.Events` | `IEventBus` / `EventBus` for events between features |
| MVC | `RapidPro.MVC` | `Model`, `View`, `Controller<TModel, TView>` bases |
| Pooling | `RapidPro.Pooling` | `IPoolService` / `PoolService`, `IPoolable`, `PoolConfig` |

## Requirements

Install these in the game's `Packages/manifest.json` / NuGetForUnity. Git URLs can't be listed as `package.json` dependencies, so they are documented here instead.

| Package | Needed by | How |
|---|---|---|
| R3 core (`R3.dll`) | Events | NuGetForUnity: `R3` |
| R3 Unity (`com.cysharp.r3`) | Unity helpers such as `AddTo(this)` | `https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity` |

The two R3 installs are not duplicates: the git package is the Unity layer and needs the NuGet `R3.dll` underneath it.

## Core

### Services

Register services by **interface type**, then look them up anywhere.

```csharp
Services.Register<IScoreService>(new ScoreService());

var score = Services.Get<IScoreService>();          // throws if missing
if (Services.TryGet<IAudioService>(out var audio))  // safe lookup
    audio.Play("click");
```

### GameInstaller

Put one installer in your first scene. Register every service by hand in `Install()` — no reflection, so you can read the setup top to bottom.

```csharp
public class MyGameInstaller : GameInstaller
{
    [SerializeField] private LevelConfig _levelConfig;

    protected override void Install()
    {
        Services.Register<IScoreService>(new ScoreService());
        Services.Register<ILevelService>(new LevelService(_levelConfig));
    }
}
```

### Log

```csharp
Log.Info("Level loaded");
Log.Warning("No save found, using defaults");
Log.Error("Missing level config");
```

`Info` and `Warning` are stripped from release builds; they compile only in the Editor, in development builds, or when `RAPIDPRO_LOG` is defined. `Error` is always kept.

## Events

Use the event bus **between** features (gameplay → UI, audio, save). Inside one feature, bind directly. Events are small structs named in the past tense.

```csharp
public readonly struct LevelCompleted
{
    public readonly int LevelIndex;
    public LevelCompleted(int levelIndex) => LevelIndex = levelIndex;
}

// GameInstaller
Services.Register<IEventBus>(new EventBus());

// Gameplay
Services.Get<IEventBus>().Publish(new LevelCompleted(3));

// Audio (MonoBehaviour)
Services.Get<IEventBus>().On<LevelCompleted>()
    .Subscribe(e => PlayWinSound())
    .AddTo(this);
```

## MVC

- **Model**: plain C#, no Unity code. State lives in private `ReactiveProperty` fields exposed read-only.
- **View**: a MonoBehaviour that shows data and exposes clicks as `Observable`. No game rules.
- **Controller**: a MonoBehaviour that connects one model to one view. The view is wired in the Inspector.

```csharp
public class ScoreModel : Model
{
    private readonly ReactiveProperty<int> _score;
    public ReadOnlyReactiveProperty<int> Score => _score;

    public ScoreModel() { _score = CreateProperty(0); }

    public void Add(int points) => _score.Value += points;
    public void Reset() => _score.Value = 0;
}

public class ScoreView : View
{
    [SerializeField] private TMP_Text _scoreLabel;
    [SerializeField] private Button _resetButton;

    public Observable<Unit> ResetClicked => _resetButton.OnClickAsObservable();
    public void ShowScore(int score) => _scoreLabel.text = score.ToString();
}

public class ScoreController : Controller<ScoreModel, ScoreView>
{
    protected override void OnBind()
    {
        Model.Score.Subscribe(View.ShowScore).AddTo(Disposables);
        View.ResetClicked.Subscribe(_ => Model.Reset()).AddTo(Disposables);
    }
}
```

**Getting the model in:**
- To share one model, call `controller.Bind(model)` from your installer or a feature bootstrapper before `Start`.
- If nothing was bound, `Start` binds `CreateModel()`, which is `new TModel()` by default. Override it to fetch a shared model, e.g. from `Services`.
- The controller never disposes the model; whoever created it does.

> **Old classes:** the arrow game still uses the earlier `BaseModel`, `BaseView`, `BaseController` and `ModelLocator` in `Assets/Framework/MVC`. They stay until the game migrates in its own step. New code should use `RapidPro.MVC`.

## Pooling

Spawn and despawn instead of `Instantiate` and `Destroy`, so next level and retry reuse objects.

1. Create a config: **Assets > Create > RapidPro > Pool Config**. Add each prefab and its prewarm count.
2. Register the service in your installer:

```csharp
[SerializeField] private PoolConfig _poolConfig;

protected override void Install()
{
    Services.Register<IPoolService>(new PoolService(_poolConfig, transform));
}
```

3. Use it:

```csharp
var pool = Services.Get<IPoolService>();

Arrow arrow = pool.Spawn(_arrowPrefab, position, Quaternion.identity);
pool.Despawn(arrow.gameObject);
```

Objects that must reset when reused implement `IPoolable`:

```csharp
public class Coin : MonoBehaviour, IPoolable
{
    public void OnSpawn()   { /* reset state */ }
    public void OnDespawn() { /* stop effects */ }
}
```

- Each prefab gets its own pool. Pooled objects wait, inactive, under `[RapidPro Pool]` in the Hierarchy.
- Despawning an object that wasn't spawned from a pool logs a warning and destroys it. Despawning twice logs a warning and is ignored.

## Rules

- Framework code never references game code (`Assets/Game`).
- One assembly per module, prefixed `RapidPro.`.
