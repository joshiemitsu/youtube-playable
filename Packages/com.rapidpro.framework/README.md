# RapidPro Framework

A small, readable framework for rapid prototyping hyper/hybrid-casual games in Unity.

## Modules

| Module | Assembly | What it gives you |
|---|---|---|
| Core | `RapidPro.Core` | `Services` (service locator), `Log`, `GameInstaller` |
| Events | `RapidPro.Events` | `IEventBus` / `EventBus` for events between features |
| MVC | `RapidPro.MVC` | `Model`, `View`, `Controller<TModel, TView>` bases |
| Pooling | `RapidPro.Pooling` | `IPoolService` / `PoolService`, `IPoolable`, `PoolConfig` |
| StateMachine | `RapidPro.StateMachine` | `StateMachine<TStateId>`, `State` for the game loop |
| Audio | `RapidPro.Audio` | `AudioManager`, `AudioCue`, `IAudioService`, `IAudioSettings` |

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

## State machine

Runs the game loop `Home → Playing → Win or Lose → Home`. States are keyed by an enum and the current state is observable, so UI can react. Needs UniTask (`https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`).

```csharp
public enum GameState { Home, Playing, Win, Lose }

public class PlayingState : State
{
    protected override void Enter() { /* start the level */ }
    public override void Tick(float deltaTime) { /* update timers */ }
    public override void Exit() { /* stop input */ }
}

public class WinState : State
{
    // Override EnterAsync instead of Enter when you need to wait, e.g. for an animation.
    public override async UniTask EnterAsync(CancellationToken cancellationToken)
    {
        await _winScreen.PlayShowAnimation(cancellationToken);
    }
}

// Setup, e.g. in a GameLoop MonoBehaviour
_loop = new StateMachine<GameState>();
_loop.Register(GameState.Home, new HomeState());
_loop.Register(GameState.Playing, new PlayingState());
_loop.Register(GameState.Win, new WinState());
_loop.Register(GameState.Lose, new LoseState());
_loop.ChangeStateAsync(GameState.Home, destroyCancellationToken).Forget();

// Update
_loop.Tick(Time.deltaTime);

// UI
_loop.CurrentState.Subscribe(state => _pauseButton.SetActive(state == GameState.Playing)).AddTo(this);
```

- A change calls `Exit` on the old state, updates `CurrentState`, then waits for the new state's `EnterAsync`.
- A change requested while another is still running is **ignored** with a warning, not queued. This stops double taps from skipping states.
- Changing to the current state exits and re-enters it, which is useful for Retry.
- `Tick` does nothing while a change is running.

## Audio

1. Add an `AudioManager` component to a GameObject in the Bootstrap scene, so music keeps playing between levels.
2. Create one cue per sound: **Assets > Create > RapidPro > Audio Cue**. Add one or more clips; one is picked at random each time. Tune volume, pitch range and the minimum gap (stops fast taps from stacking).
3. Register it once per interface in your installer:

```csharp
[SerializeField] private AudioManager _audio;

protected override void Install()
{
    Services.Register<IAudioService>(_audio);   // gameplay and UI
    Services.Register<IAudioSettings>(_audio);  // Settings screen only
}
```

4. Play sounds:

```csharp
var audio = Services.Get<IAudioService>();
audio.PlayBgm(_gameMusic);                // crossfades from the current track
audio.PlaySfx(_tapCue);                   // 2D
audio.PlaySfx(_exitCue, arrow.position);  // 3D at a world position
```

- Music crossfades between two sources. Playing the cue that is already playing does nothing.
- Sound effects use up to 8 sources. When all are busy, the one closest to finishing is reused.
- `IAudioSettings` exposes `MusicVolume` and `SfxVolume` (0 to 1) for sliders. Volumes live in memory for now; the Save module will persist them.

## Rules

- Framework code never references game code (`Assets/Game`).
- One assembly per module, prefixed `RapidPro.`.
