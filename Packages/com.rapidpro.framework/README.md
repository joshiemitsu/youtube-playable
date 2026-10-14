# RapidPro Framework

A small, readable framework for rapid prototyping hyper/hybrid-casual games in Unity.

## Modules

| Module | Assembly | What it gives you |
|---|---|---|
| Core | `RapidPro.Core` | `Services` (service locator), `Log`, `GameInstaller` |
| Events | `RapidPro.Events` | `IEventBus` / `EventBus` for events between features |

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

## Rules

- Framework code never references game code (`Assets/Game`).
- One assembly per module, prefixed `RapidPro.`.
