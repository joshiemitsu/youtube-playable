using Cysharp.Threading.Tasks;
using R3;
using RapidPro.Core;
using RapidPro.GameLoop;
using UnityEngine;

/// <summary>
/// Shows the win or lose screen when a level ends, and restarts the level from either button.
/// </summary>
public class GameResultController : MonoBehaviour
{
    [SerializeField] private GameResultView _winView;
    [SerializeField] private GameResultView _loseView;

    private IGameLoop _loop;

    // Start, not Awake: the Game scene registers IGameLoop before this GameUI scene starts
    private void Start()
    {
        _winView.Hide();
        _loseView.Hide();

        if (!Services.TryGet(out _loop))
        {
            Log.Warning("GameResultController: no IGameLoop registered. Start the game from the Bootstrap scene.", this);
            return;
        }

        _loop.State.Subscribe(ShowResult).AddTo(this);

        _winView.RestartClicked
            .Merge(_loseView.RestartClicked)
            .Subscribe(_ => _loop.RestartAsync().Forget())
            .AddTo(this);
    }

    private void ShowResult(GameState p_state)
    {
        if (p_state == GameState.Win) _winView.Show(); else _winView.Hide();
        if (p_state == GameState.Lose) _loseView.Show(); else _loseView.Hide();
    }
}
