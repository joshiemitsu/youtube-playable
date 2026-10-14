using NUnit.Framework;
using R3;
using UnityEditor;
using UnityEngine;

namespace RapidPro.MVC.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="Model"/> and <see cref="Controller{TModel, TView}"/>.
    /// </summary>
    public class MvcTests
    {
        private GameObject _gameObject;
        private TestScoreView _view;
        private TestScoreController _controller;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("MVC Test");
            _view = _gameObject.AddComponent<TestScoreView>();
            _controller = _gameObject.AddComponent<TestScoreController>();

            // Wire the view the same way the Inspector does.
            var serialized = new SerializedObject(_controller);
            serialized.FindProperty("_view").objectReferenceValue = _view;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Unbind();
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Model_ExposesStateReadOnly()
        {
            var property = typeof(TestScoreModel).GetProperty(nameof(TestScoreModel.Score));

            Assert.AreEqual(typeof(ReadOnlyReactiveProperty<int>), property.PropertyType);
            Assert.IsFalse(property.CanWrite);
        }

        [Test]
        public void Model_Dispose_CompletesProperties()
        {
            var model = new TestScoreModel();
            bool completed = false;
            model.Score.Subscribe(_ => { }, _ => completed = true);

            model.Dispose();

            Assert.IsTrue(completed);
        }

        [Test]
        public void Bind_ShowsModelOnViewAndFollowsChanges()
        {
            var model = new TestScoreModel();
            model.Add(3);

            _controller.Bind(model);
            Assert.AreEqual(3, _view.ShownScore);

            model.Add(2);
            Assert.AreEqual(5, _view.ShownScore);
        }

        [Test]
        public void ViewClick_ChangesModel()
        {
            var model = new TestScoreModel();
            model.Add(4);
            _controller.Bind(model);

            _view.ClickReset();

            Assert.AreEqual(0, model.Score.CurrentValue);
            Assert.AreEqual(0, _view.ShownScore);
        }

        [Test]
        public void Start_WithoutBind_CreatesAndBindsModel()
        {
            _controller.RunStart();

            Assert.IsTrue(_controller.IsBound);
            Assert.AreEqual(0, _view.ShownScore);

            _controller.BoundModel.Add(1);
            Assert.AreEqual(1, _view.ShownScore);
        }

        [Test]
        public void Start_AfterBind_KeepsBoundModel()
        {
            var shared = new TestScoreModel();
            _controller.Bind(shared);

            _controller.RunStart();

            Assert.AreSame(shared, _controller.BoundModel);
        }

        [Test]
        public void Unbind_StopsUpdates()
        {
            var model = new TestScoreModel();
            _controller.Bind(model);

            _controller.Unbind();
            model.Add(7);

            Assert.IsFalse(_controller.IsBound);
            Assert.AreEqual(0, _view.ShownScore);
        }

        [Test]
        public void Bind_NewModel_DisposesOldSubscriptions()
        {
            var oldModel = new TestScoreModel();
            var newModel = new TestScoreModel();
            _controller.Bind(oldModel);
            _controller.Bind(newModel);

            oldModel.Add(9);

            Assert.AreEqual(0, _view.ShownScore);
        }
    }
}
