# RapidPro Framework

A small, readable framework for rapid prototyping hyper/hybrid-casual games in Unity.

## Modules

| Module | Assembly | What it gives you |
|---|---|---|
| Core | `RapidPro.Core` | `Services` (service locator), `Log`, `GameInstaller` |

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

## Rules

- Framework code never references game code (`Assets/Game`).
- One assembly per module, prefixed `RapidPro.`.
