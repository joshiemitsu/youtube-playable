using R3;
using RapidPro.Core;
using UnityEngine;

/// <summary>
/// Keeps the hearts HUD in step with the game's hearts.
/// </summary>
public class HeartsController : MonoBehaviour
{
    [SerializeField] private HeartsView _view;

    // Start, not Awake: the Game scene registers IHearts before this GameUI scene starts
    private void Start()
    {
        if (!Services.TryGet(out IHearts hearts))
        {
            Log.Warning("HeartsController: no IHearts registered, so the hearts HUD stays hidden.", this);
            _view.Hide();
            return;
        }

        hearts.Current
            .Subscribe(current => _view.ShowHearts(current, hearts.Max))
            .AddTo(this);
    }
}
