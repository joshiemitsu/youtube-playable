using System;
using R3;

namespace RapidPro.MVC
{
    /// <summary>
    /// Base class for a model: plain C# state with no Unity code, so it is easy to test.
    /// Keep each value in a private <see cref="ReactiveProperty{T}"/> made with <see cref="CreateProperty{T}"/>
    /// and expose it read-only. Change state only through the model's own methods.
    /// </summary>
    /// <example>
    /// <code>
    /// public class ScoreModel : Model
    /// {
    ///     private readonly ReactiveProperty&lt;int&gt; _score;
    ///     public ReadOnlyReactiveProperty&lt;int&gt; Score => _score;
    ///
    ///     public ScoreModel() { _score = CreateProperty(0); }
    ///
    ///     public void Add(int points) => _score.Value += points;
    /// }
    /// </code>
    /// </example>
    public abstract class Model : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>
        /// Creates a property that this model disposes for you in <see cref="Dispose"/>.
        /// </summary>
        /// <param name="initialValue">The starting value.</param>
        protected ReactiveProperty<T> CreateProperty<T>(T initialValue)
        {
            var property = new ReactiveProperty<T>(initialValue);
            _disposables.Add(property);
            return property;
        }

        /// <summary>
        /// Completes and disposes every property made with <see cref="CreateProperty{T}"/>.
        /// Call it from whoever created the model, not from controllers that only use it.
        /// </summary>
        public virtual void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
