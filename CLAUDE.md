# YT-Playables

Unity 6 (6000.3) project. Game code lives in `Assets/Game`. Reusable framework code lives in the embedded package `Packages/com.rapidpro.framework`.

## Framework rules

- Framework code lives in `Packages/com.rapidpro.framework` and **never** references `Assets/Game`. Dependencies point one way: game → framework.
- One asmdef per module, prefixed `RapidPro.` (e.g. `RapidPro.Core`, `RapidPro.Core.Tests`).
- Every file: one public type, XML doc comments on public API, `_camelCase` private fields, `[SerializeField] private` instead of public fields.
- Register services by interface type, by hand, in a `GameInstaller` subclass. No reflection auto-wiring.
- Log through `RapidPro.Core.Log`, not `Debug.Log`.
- SOLID, but readability wins ties. Code should be easy for a mid-level developer to follow.

## Making a game on the framework

The arrow game in `Assets/Game` is the reference. A new game needs only:
1. A `LevelData` subclass + a `LevelCatalog` asset (see `Assets/Game/Levels/Scripts/ArrowLevelData.cs`).
2. One `IGameMode` MonoBehaviour that builds the level, listens to input and calls `IGameLoop.Win()` / `Lose()` (see `Assets/Game/GameMode/ArrowGameMode.cs`).
3. A prefab variant of `Packages/com.rapidpro.framework/Runtime/GameLoop/Prefabs/GameRoot.prefab` holding the game mode and board (see `Assets/Game/Prefabs/ArrowGame.prefab`). Never edit GameRoot itself.

Services are registered in the Bootstrap scene by `Assets/Game/Bootstrap/GameServicesInstaller.cs`. UI reads `IGameLoop.State` and game-specific read-only interfaces (e.g. `IHearts`), never the game mode directly. Read `Packages/com.rapidpro.framework/README.md` for each module's API before searching the code.

## R3 rules

- Never expose a public `Subject`. Expose `Observable<T>` instead.
- Expose state as a read-only `ReadOnlyReactiveProperty<T>`; keep the writable `ReactiveProperty<T>` private.
- Always dispose subscriptions with `.AddTo(...)`.
- Use `SubscribeAwait` with an explicit `AwaitOperation` (e.g. `AwaitOperation.Drop`, `AwaitOperation.Sequential`).

## Testing and done

- With the Editor open, run tests through the CLI: `unity command run_tests --project-path D:\YT-Playables`. Plain `unity test` only works while Unity is **closed**.
- Check compile errors with `unity command console` before running tests.
- Work is not done until it compiles with zero errors and all tests pass.
