using UnityEngine;

namespace RapidPro.MVC
{
    /// <summary>
    /// Base class for a view: shows data and exposes user input, with no game rules.
    /// Give it public methods to display values, and expose buttons as <c>Observable</c> streams.
    /// </summary>
    /// <example>
    /// <code>
    /// public class ScoreView : View
    /// {
    ///     [SerializeField] private TMP_Text _scoreLabel;
    ///     [SerializeField] private Button _resetButton;
    ///
    ///     public Observable&lt;Unit&gt; ResetClicked => _resetButton.OnClickAsObservable();
    ///
    ///     public void ShowScore(int score) => _scoreLabel.text = score.ToString();
    /// }
    /// </code>
    /// </example>
    public abstract class View : MonoBehaviour
    {
        /// <summary>Shows the view. Override to add a transition.</summary>
        public virtual void Show() => gameObject.SetActive(true);

        /// <summary>Hides the view. Override to add a transition.</summary>
        public virtual void Hide() => gameObject.SetActive(false);
    }
}
