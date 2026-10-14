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
| Save | `RapidPro.Save` | `ISaveService` / `SaveService` key-value save file |
| Lifecycle | `RapidPro.Lifecycle` | `IPauseService` / `PauseService`, `IAppLifecycle` / `AppLifecycle` |
| Display | `RapidPro.Display` | `SafeAreaFitter`, `CanvasFitter`, `CameraFitter` |
| Scenes | `RapidPro.Scenes` (+ `RapidPro.Scenes.Editor`) | `SceneFlow`, `SceneFlowSettings`, `SceneFlowStarter`, `LoadingProgressBar`, play from any scene |

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
3. Connect it to the save service and register it once per interface in your installer:

```csharp
[SerializeField] private AudioManager _audio;

protected override void Install()
{
    var save = new SaveService();
    Services.Register<ISaveService>(save);

    _audio.UseSave(save);                       // loads and saves music/SFX volume
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
- `IAudioSettings` exposes `MusicVolume` and `SfxVolume` (0 to 1) for sliders. After `UseSave`, they are saved under `audio.musicVolume` and `audio.sfxVolume`. Without it they reset on restart.

## Save

Stores small player data under string keys in one JSON file (`Application.persistentDataPath/save.json`).

```csharp
var save = Services.Get<ISaveService>();

int level = save.Load("progress.level", 1);   // 1 when nothing is saved yet
save.Save("progress.level", level + 1);       // memory only, cheap
save.Flush();                                 // write to disk, e.g. after a level
```

- Values can be `int`, `float`, `bool`, `string`, or any `[Serializable]` class or struct.
- Name keys by feature: `audio.musicVolume`, `progress.level`, `settings.haptics`.
- `Save` only changes memory, so calling it from a slider is fine. `Flush` writes the file; it also runs when the app quits, and when the app goes to the background once `AppLifecycle` is connected (see below).
- Writes go to a temp file first, then replace the real one, so a crash mid-write cannot corrupt the save.
- A save file that can't be read logs a warning and the game starts fresh.
- `DeleteAll()` wipes everything, for "Reset save" in the debug panel.

## Pause and app lifecycle

**Pause** works by reason. The game stays paused until every reason is resumed, so closing a popup does not unpause a game the player paused.

```csharp
_pause.Pause(PauseReasons.Popup);    // Time.timeScale = 0
_pause.Resume(PauseReasons.Popup);   // restores the previous timeScale when no reasons are left

_pause.IsPaused.Subscribe(paused => _pauseOverlay.SetActive(paused)).AddTo(this);
```

Anything that must keep moving while paused (UI tweens, music fades) should use unscaled time.

**App lifecycle**: add an `AppLifecycle` component to the Bootstrap scene and connect it:

```csharp
[SerializeField] private AppLifecycle _lifecycle;

protected override void Install()
{
    var save = new SaveService();
    var pause = new PauseService();
    Services.Register<ISaveService>(save);
    Services.Register<IPauseService>(pause);

    _lifecycle.Connect(pause, save);
    Services.Register<IAppLifecycle>(_lifecycle);
}
```

- **Going to the background**: pauses (`PauseReasons.Background`), flushes the save, then fires `WentToBackground`.
- **Coming back**: fires `ReturnedFromBackground`. With **Resume On Return** ticked (default) the game resumes at once. Untick it once a "tap to continue" overlay calls `Resume(PauseReasons.Background)` itself.
- **Back button** (Android Back, Escape on desktop): fires `BackPressed`. The UI module will use it to close the top popup or ask to quit.
- Only real backgrounding counts; clicking another window in the Editor does not pause the game.

## Safe area and camera fit

Three components that make the game fit any screen: phones, tablets, WebGL in a resizable browser, and desktop windows. Each one checks the screen every frame and refits only when the size, orientation or safe area changes. All settings can be tweaked in Play Mode and apply at once. Each also has **Fit Now** in its ⋮ menu.

**CanvasFitter**: add it next to the Canvas Scaler on each Canvas.

| Setting | What it does |
|---|---|
| Mode | **Fit Inside** (default): the whole layout always fits, with extra space around it. **Fill**: the layout fills the screen, edges may be cut. **Weighted**: use the sliders below. |
| Portrait / Landscape Reference | The resolution the UI was designed for in each orientation. Switches by itself when the screen turns. |
| Width Or Height | Weighted only. 0 = match the screen width, 1 = match the height. |

**SafeAreaFitter**: add it to a full-screen panel under the Canvas and put the screen's content inside. Tick which edges to keep clear (top notch, bottom home bar, left/right in landscape). Backgrounds go outside the panel so they still fill the whole screen.

**CameraFitter**: add it to the camera. It fits the level so it is fully visible on any screen shape.

| Setting | What it does |
|---|---|
| Bounds | The world area that must stay visible. Set it here, or from code after a level loads (below). Shown as a cyan box when selected. |
| Padding | Extra world units around the level. |
| Mode | **Fit Inside** (default): the whole level is always visible. **Weighted**: blend between fitting width and height. |
| Width Or Height | Weighted only. 0 = fit the level's width, 1 = fit its height. |
| Anchor | Where the level sits when there is spare room. (0.5, 0.5) = centre, (0.5, 0) = bottom, (0.5, 1) = top. |
| Use Safe Area | Keep the level out of notches. |
| Reserved Top / Bottom | Part of the screen height kept free, e.g. 0.1 for a HUD on top or a banner ad at the bottom. |

- **Orthographic camera**: changes the orthographic size.
- **Perspective camera**: moves the camera along its view direction (the z axis for a camera looking straight ahead) until the level fits. Works for top-down and tilted cameras too. The rotation is never changed.

```csharp
// After a level loads, e.g. a board of columns x rows cells centred at boardCenter
_cameraFitter.SetBounds(new Bounds(boardCenter, new Vector3(columns, rows, 0f)));
```

## Bootstrap and scene flow

How a game starts:

```
Bootstrap (stays loaded) ──► Loading (optional, logo + bar) ──► Game + GameUI ──► Loading unloads
```

- **Bootstrap** holds your `GameInstaller` and every service. It never unloads, so nothing needs `DontDestroyOnLoad`.
- **Game** is one scene for every level. **GameUI** loads on top of it. Game becomes the active scene, so spawned objects land there.

**Setup**

1. **Assets > Create > RapidPro > Scene Flow Settings**, saved as `Assets/Resources/SceneFlowSettings.asset`. Fill in the scene names. Leave Loading empty if you have no Loading scene. The Console warns if a named scene is missing from the build scene list.
2. In the Bootstrap scene, register the flow in your installer and add a **SceneFlowStarter** component:

```csharp
[SerializeField] private SceneFlowSettings _sceneFlowSettings;

protected override void Install()
{
    Services.Register<ISceneFlow>(new SceneFlow(_sceneFlowSettings));
    // ... other services
}
```

3. Optional Loading scene: put a **LoadingProgressBar** on a filled Image.
4. Put Bootstrap first in the build scene list.

**Play from any scene (Editor)**: with the settings asset in Resources and *Play From Any Scene* ticked, pressing Play always starts from Bootstrap, so services exist everywhere.
- Playing in Game, GameUI, Loading or Bootstrap starts the game normally.
- Playing in any other scene (e.g. a sandbox or the level editor scene) loads Bootstrap, then opens **only that scene**, even if it is not in the build list.
- Unity asks to save modified scenes first, because Play loads scenes from disk.
- Untick the setting, or delete the asset, to play scenes on their own again.

> **Old class:** `Assets/Framework/Boostrap/Boostrap.cs` (loads Game and GameUI, then unloads Bootstrap) is replaced by `SceneFlow`. Remove it when the arrow game migrates.

## Rules

- Framework code never references game code (`Assets/Game`).
- One assembly per module, prefixed `RapidPro.`.
