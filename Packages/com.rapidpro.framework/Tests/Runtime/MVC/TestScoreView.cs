using R3;

namespace RapidPro.MVC.Tests
{
    /// <summary>Test view that records what it shows and fakes a reset button.</summary>
    public class TestScoreView : View
    {
        private readonly Subject<Unit> _resetClicked = new Subject<Unit>();

        /// <summary>The last score shown.</summary>
        public int ShownScore { get; private set; } = -1;

        /// <summary>Fires when the reset button is clicked.</summary>
        public Observable<Unit> ResetClicked => _resetClicked;

        /// <summary>Shows a score.</summary>
        public void ShowScore(int score) => ShownScore = score;

        /// <summary>Simulates a click on the reset button.</summary>
        public void ClickReset() => _resetClicked.OnNext(Unit.Default);

        private void OnDestroy() => _resetClicked.Dispose();
    }
}
