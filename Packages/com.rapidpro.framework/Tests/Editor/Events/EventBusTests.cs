using System.Collections.Generic;
using NUnit.Framework;
using R3;

namespace RapidPro.Events.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="EventBus"/>.
    /// </summary>
    public class EventBusTests
    {
        private readonly struct LevelCompleted
        {
            public readonly int LevelIndex;
            public LevelCompleted(int levelIndex) => LevelIndex = levelIndex;
        }

        private readonly struct LevelFailed { }

        private EventBus _bus;
        private CompositeDisposable _disposables;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _disposables = new CompositeDisposable();
        }

        [TearDown]
        public void TearDown()
        {
            _disposables.Dispose();
            _bus.Dispose();
        }

        [Test]
        public void Publish_WithSubscriber_DeliversEvent()
        {
            var received = new List<int>();
            _bus.On<LevelCompleted>().Subscribe(e => received.Add(e.LevelIndex)).AddTo(_disposables);

            _bus.Publish(new LevelCompleted(3));

            CollectionAssert.AreEqual(new[] { 3 }, received);
        }

        [Test]
        public void Publish_OtherEventType_IsNotDelivered()
        {
            int count = 0;
            _bus.On<LevelCompleted>().Subscribe(_ => count++).AddTo(_disposables);

            _bus.Publish(new LevelFailed());

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Publish_AfterSubscriptionDisposed_IsNotDelivered()
        {
            int count = 0;
            var subscription = _bus.On<LevelCompleted>().Subscribe(_ => count++);

            subscription.Dispose();
            _bus.Publish(new LevelCompleted(1));

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Publish_WithMultipleSubscribers_DeliversToAll()
        {
            int first = 0, second = 0;
            _bus.On<LevelCompleted>().Subscribe(_ => first++).AddTo(_disposables);
            _bus.On<LevelCompleted>().Subscribe(_ => second++).AddTo(_disposables);

            _bus.Publish(new LevelCompleted(1));

            Assert.AreEqual(1, first);
            Assert.AreEqual(1, second);
        }
    }
}
