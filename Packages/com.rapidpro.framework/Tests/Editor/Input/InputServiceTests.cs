using System;
using System.Collections.Generic;
using NUnit.Framework;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RapidPro.Input.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="InputService"/> and its gestures, driven by a fake pointer source on a
    /// 1000 x 2000 screen with the default <see cref="InputConfig"/>:
    /// drag threshold 20 px, hold 0.5 s, double tap 0.3 s / 60 px, flick 1500 px/s within 0.35 s.
    /// </summary>
    public class InputServiceTests
    {
        /// <summary>A pretend touch screen. Put fingers down, move them, lift them, scroll.</summary>
        private class FakePointerSource : IPointerSource
        {
            private readonly Dictionary<int, Vector2> _down = new Dictionary<int, Vector2>();
            public float Scroll;

            public Vector2 ScreenSize => new Vector2(1000f, 2000f);

            public void Down(int id, float x, float y) => _down[id] = new Vector2(x, y);
            public void Up(int id) => _down.Remove(id);

            public void ReadPointers(List<RawPointer> pointers)
            {
                foreach (var pair in _down)
                    pointers.Add(new RawPointer(pair.Key, pair.Value));
            }

            public float ReadScroll()
            {
                float scroll = Scroll;
                Scroll = 0f;
                return scroll;
            }

            public Vector2 ReadCursorPosition() => new Vector2(500f, 1000f);
        }

        /// <summary>A custom gesture that counts frames, to prove recognizers can be added.</summary>
        private class FrameCounter : IGestureRecognizer
        {
            public int Frames;
            public void Process(PointerFrame frame, InputConfig config) => Frames++;
            public void Reset() { }
        }

        private GameObject _gameObject;
        private InputService _input;
        private FakePointerSource _source;
        private InputConfig _config;
        private CompositeDisposable _subscriptions;
        private float _time;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("Input Test");
            _input = _gameObject.AddComponent<InputService>();
            _config = ScriptableObject.CreateInstance<InputConfig>();
            _input.SetConfig(_config);
            _source = new FakePointerSource();
            _input.SetSource(_source);
            _subscriptions = new CompositeDisposable();
            _time = 10f;
        }

        [TearDown]
        public void TearDown()
        {
            _subscriptions.Dispose();
            Object.DestroyImmediate(_gameObject);
            Object.DestroyImmediate(_config);
        }

        private void Step(float seconds = 0.016f)
        {
            _time += seconds;
            _input.Tick(_time);
        }

        private List<T> Record<T>(Observable<T> stream)
        {
            var list = new List<T>();
            stream.Subscribe(list.Add).AddTo(_subscriptions);
            return list;
        }

        private void Tap(float x, float y)
        {
            _source.Down(0, x, y);
            Step();
            _source.Up(0);
            Step(0.05f);
        }

        [Test]
        public void QuickTouch_IsTapAndRelease()
        {
            var taps = Record(_input.Tapped);
            var presses = Record(_input.Pressed);
            var releases = Record(_input.Released);

            Tap(100f, 200f);

            Assert.AreEqual(1, taps.Count);
            Assert.AreEqual(new Vector2(100f, 200f), taps[0].Position);
            Assert.AreEqual(1, presses.Count);
            Assert.AreEqual(1, releases.Count);
            Assert.IsFalse(releases[0].WasDrag);
        }

        [Test]
        public void LongTouch_IsHoldNotTap()
        {
            var taps = Record(_input.Tapped);
            var holds = Record(_input.Held);

            _source.Down(0, 100f, 200f);
            Step();
            Step(0.3f);
            Assert.AreEqual(0, holds.Count, "Not held long enough yet.");
            Step(0.3f);
            Step(0.1f);
            _source.Up(0);
            Step();

            Assert.AreEqual(1, holds.Count, "Hold fires once.");
            Assert.AreEqual(0, taps.Count);
        }

        [Test]
        public void TwoQuickTaps_AreDoubleTap()
        {
            var taps = Record(_input.Tapped);
            var doubleTaps = Record(_input.DoubleTapped);

            Tap(100f, 200f);
            Step(0.1f);
            Tap(110f, 205f);

            Assert.AreEqual(2, taps.Count);
            Assert.AreEqual(1, doubleTaps.Count);
            Assert.AreEqual(2, doubleTaps[0].TapCount);
        }

        [Test]
        public void TwoTapsFarApart_AreNotDoubleTap()
        {
            var doubleTaps = Record(_input.DoubleTapped);

            Tap(100f, 200f);
            Tap(600f, 900f);

            Assert.AreEqual(0, doubleTaps.Count);
        }

        [Test]
        public void SlowMove_IsDragNotTap()
        {
            var taps = Record(_input.Tapped);
            var started = Record(_input.DragStarted);
            var dragged = Record(_input.Dragged);
            var ended = Record(_input.DragEnded);
            var flicks = Record(_input.Flicked);

            _source.Down(0, 100f, 200f);
            Step();
            _source.Down(0, 150f, 200f);
            Step(0.2f);
            _source.Down(0, 200f, 200f);
            Step(0.2f);
            _source.Up(0);
            Step(0.2f);

            Assert.AreEqual(1, started.Count);
            Assert.AreEqual(1, dragged.Count);
            Assert.AreEqual(new Vector2(50f, 0f), dragged[0].Delta);
            Assert.AreEqual(1, ended.Count);
            Assert.AreEqual(new Vector2(100f, 0f), ended[0].TotalDelta);
            Assert.AreEqual(0, taps.Count);
            Assert.AreEqual(0, flicks.Count, "Too slow to be a flick.");
        }

        [Test]
        public void FastSwipe_IsFlickInItsDirection()
        {
            var flicks = Record(_input.Flicked);

            _source.Down(0, 100f, 200f);
            Step();
            _source.Down(0, 100f, 500f);
            Step(0.1f);
            _source.Up(0);
            Step(0.016f);

            Assert.AreEqual(1, flicks.Count);
            Assert.AreEqual(FlickDirection.Up, flicks[0].Direction);
        }

        [Test]
        public void FingersApart_IsPinchOut_AndNotATap()
        {
            var pinchesOut = Record(_input.PinchedOut);
            var pinchesIn = Record(_input.PinchedIn);
            var taps = Record(_input.Tapped);

            _source.Down(0, 400f, 1000f);
            _source.Down(1, 600f, 1000f);
            Step();
            _source.Down(0, 300f, 1000f);
            _source.Down(1, 700f, 1000f);
            Step();
            _source.Up(0);
            _source.Up(1);
            Step();

            Assert.AreEqual(1, pinchesOut.Count);
            Assert.AreEqual(2f, pinchesOut[0].Scale, 0.001f, "Fingers went from 200 px to 400 px apart.");
            Assert.AreEqual(0, pinchesIn.Count);
            Assert.AreEqual(0, taps.Count);
        }

        [Test]
        public void FingersTogether_IsPinchIn()
        {
            var pinchesIn = Record(_input.PinchedIn);

            _source.Down(0, 300f, 1000f);
            _source.Down(1, 700f, 1000f);
            Step();
            _source.Down(0, 450f, 1000f);
            _source.Down(1, 550f, 1000f);
            Step();

            Assert.AreEqual(1, pinchesIn.Count);
            Assert.Less(pinchesIn[0].Delta, 0f);
        }

        [Test]
        public void ScrollWheel_IsPinch()
        {
            var pinchesOut = Record(_input.PinchedOut);

            _source.Scroll = 1f;
            Step();

            Assert.AreEqual(1, pinchesOut.Count);
            Assert.AreEqual(0.1f, pinchesOut[0].Delta, 0.0001f);
        }

        [Test]
        public void Blocked_IgnoresEverything_AndFingerHeldThroughUnblockIsIgnored()
        {
            var taps = Record(_input.Tapped);
            var presses = Record(_input.Pressed);

            _input.Block("popup");
            Tap(100f, 200f);
            Assert.AreEqual(0, presses.Count);

            _source.Down(0, 100f, 200f);
            Step();
            _input.Unblock("popup");
            Step();
            _source.Up(0);
            Step();

            Assert.AreEqual(0, taps.Count, "A finger already down when input unblocks must not tap.");

            Tap(100f, 200f);
            Assert.AreEqual(1, taps.Count, "Input works again after the finger was lifted.");
        }

        [Test]
        public void TwoBlockReasons_StayBlockedUntilBothRemoved()
        {
            _input.Block("popup");
            _input.Block("transition");
            _input.Unblock("popup");

            Assert.IsTrue(_input.IsBlocked.CurrentValue);

            _input.Unblock("transition");
            Assert.IsFalse(_input.IsBlocked.CurrentValue);
        }

        [Test]
        public void CustomRecognizer_ReceivesFrames()
        {
            var counter = new FrameCounter();
            _input.AddRecognizer(counter);

            Step();
            Step();
            _input.RemoveRecognizer(counter);
            Step();

            Assert.AreEqual(2, counter.Frames);
        }
    }
}
