using R3;
using UnityEngine;

namespace RapidPro.MVC
{
    /// <summary>
    /// Base class for a controller: connects one model to one view.
    /// The view is wired in the Inspector. The model is passed in with <see cref="Bind"/>,
    /// or created in <see cref="Start"/> by <see cref="CreateModel"/> when nothing was bound.
    /// </summary>
    /// <remarks>
    /// Subscribe in <see cref="OnBind"/> and end every subscription with <c>.AddTo(Disposables)</c>.
    /// They are disposed on <see cref="Unbind"/>, on a new <see cref="Bind"/>, and on destroy.
    /// The controller does not dispose the model, because other controllers may share it.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ScoreController : Controller&lt;ScoreModel, ScoreView&gt;
    /// {
    ///     protected override void OnBind()
    ///     {
    ///         Model.Score.Subscribe(View.ShowScore).AddTo(Disposables);
    ///         View.ResetClicked.Subscribe(_ => Model.Reset()).AddTo(Disposables);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="TModel">The model type. Needs a parameterless constructor for <see cref="CreateModel"/>.</typeparam>
    /// <typeparam name="TView">The view type, assigned in the Inspector.</typeparam>
    public abstract class Controller<TModel, TView> : MonoBehaviour
        where TModel : Model, new()
        where TView : View
    {
        [SerializeField] private TView _view;

        private CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>The bound model, or null before binding.</summary>
        protected TModel Model { get; private set; }

        /// <summary>The view assigned in the Inspector.</summary>
        protected TView View => _view;

        /// <summary>Subscriptions made in <see cref="OnBind"/>. Disposed on unbind.</summary>
        protected CompositeDisposable Disposables => _disposables;

        /// <summary>True when a model is bound.</summary>
        public bool IsBound => Model != null;

        /// <summary>
        /// Connects <paramref name="model"/> to the view. Replaces any model bound before.
        /// Use this to share one model between several controllers.
        /// </summary>
        /// <param name="model">The model to show.</param>
        public void Bind(TModel model)
        {
            Unbind();
            Model = model;
            OnBind();
        }

        /// <summary>
        /// Disposes the subscriptions and clears the model. Safe to call when nothing is bound.
        /// </summary>
        public void Unbind()
        {
            if (Model == null)
                return;

            _disposables.Dispose();
            _disposables = new CompositeDisposable();
            Model = null;
        }

        /// <summary>
        /// Binds a model from <see cref="CreateModel"/> if <see cref="Bind"/> was not called before Start.
        /// If you override this, call <c>base.Start()</c>.
        /// </summary>
        protected virtual void Start()
        {
            if (!IsBound)
                Bind(CreateModel());
        }

        /// <summary>Unbinds when the GameObject is destroyed. If you override this, call <c>base.OnDestroy()</c>.</summary>
        protected virtual void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// Makes the model used when nothing was bound. Defaults to <c>new TModel()</c>.
        /// Override to get a shared model instead, e.g. from <c>Services</c>.
        /// </summary>
        protected virtual TModel CreateModel() => new TModel();

        /// <summary>
        /// Called after a model is bound. Subscribe model → view and view → model here.
        /// </summary>
        protected abstract void OnBind();
    }
}
