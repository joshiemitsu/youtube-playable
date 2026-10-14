using R3;

namespace RapidPro.MVC.Tests
{
    /// <summary>Test model with one read-only score.</summary>
    public class TestScoreModel : Model
    {
        private readonly ReactiveProperty<int> _score;

        /// <summary>The current score.</summary>
        public ReadOnlyReactiveProperty<int> Score => _score;

        /// <summary>Creates a model with a score of 0.</summary>
        public TestScoreModel()
        {
            _score = CreateProperty(0);
        }

        /// <summary>Adds points to the score.</summary>
        public void Add(int points) => _score.Value += points;

        /// <summary>Sets the score back to 0.</summary>
        public void Reset() => _score.Value = 0;
    }
}
