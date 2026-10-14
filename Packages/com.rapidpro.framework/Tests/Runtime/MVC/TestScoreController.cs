using R3;

namespace RapidPro.MVC.Tests
{
    /// <summary>Test controller that connects <see cref="TestScoreModel"/> to <see cref="TestScoreView"/>.</summary>
    public class TestScoreController : Controller<TestScoreModel, TestScoreView>
    {
        /// <summary>The bound model, for assertions.</summary>
        public TestScoreModel BoundModel => Model;

        /// <summary>Runs Start, which Unity does not call in edit mode.</summary>
        public void RunStart() => Start();

        /// <inheritdoc />
        protected override void OnBind()
        {
            Model.Score.Subscribe(View.ShowScore).AddTo(Disposables);
            View.ResetClicked.Subscribe(_ => Model.Reset()).AddTo(Disposables);
        }
    }
}
