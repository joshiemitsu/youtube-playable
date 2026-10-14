using R3;
using RapidPro.MVC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A result screen: a banner ("YOU WIN!" or "YOU LOSE") and a restart button at the bottom.
/// One instance per result, each styled in its own prefab or scene object.
/// </summary>
public class GameResultView : View
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private Button _restartButton;

    public Observable<Unit> RestartClicked => _restartButton.OnClickAsObservable();

    public void SetTitle(string p_title) => _title.text = p_title;
}
