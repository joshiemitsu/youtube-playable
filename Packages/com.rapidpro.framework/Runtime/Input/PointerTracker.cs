using System;
using System.Collections.Generic;
using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// Turns "which pointers are down" from a source into <see cref="TrackedPointer"/>s with phases and history,
    /// by comparing this frame with the last one.
    /// </summary>
    internal class PointerTracker
    {
        private class Tracked
        {
            public Vector2 Position;
            public Vector2 StartPosition;
            public float StartTime;
            public float MaxDistance;
            public bool WasMultiTouch;
            public bool StartedOverUI;
            public bool SeenThisFrame;
        }

        private readonly Dictionary<int, Tracked> _tracked = new Dictionary<int, Tracked>();
        private readonly List<int> _endedIds = new List<int>();
        private readonly Func<int, bool> _isOverUI;

        /// <param name="isOverUI">Answers whether a pointer id is over UI right now.</param>
        public PointerTracker(Func<int, bool> isOverUI)
        {
            _isOverUI = isOverUI;
        }

        /// <summary>Fills <paramref name="frame"/> from this frame's raw pointers.</summary>
        public void Update(List<RawPointer> raw, PointerFrame frame)
        {
            foreach (var tracked in _tracked.Values)
                tracked.SeenThisFrame = false;

            bool multiTouch = raw.Count > 1;

            foreach (var pointer in raw)
            {
                if (_tracked.TryGetValue(pointer.Id, out var tracked))
                {
                    var delta = pointer.Position - tracked.Position;
                    tracked.Position = pointer.Position;
                    tracked.MaxDistance = Mathf.Max(tracked.MaxDistance, Vector2.Distance(pointer.Position, tracked.StartPosition));
                    tracked.WasMultiTouch |= multiTouch;
                    tracked.SeenThisFrame = true;

                    var phase = delta == Vector2.zero ? PointerPhase.Stationary : PointerPhase.Moved;
                    frame.Add(ToPointer(pointer.Id, phase, delta, tracked));
                }
                else
                {
                    tracked = new Tracked
                    {
                        Position = pointer.Position,
                        StartPosition = pointer.Position,
                        StartTime = frame.Time,
                        WasMultiTouch = multiTouch,
                        StartedOverUI = _isOverUI(pointer.Id),
                        SeenThisFrame = true
                    };
                    _tracked.Add(pointer.Id, tracked);
                    frame.Add(ToPointer(pointer.Id, PointerPhase.Began, Vector2.zero, tracked));
                }
            }

            _endedIds.Clear();
            foreach (var pair in _tracked)
            {
                if (!pair.Value.SeenThisFrame)
                {
                    frame.Add(ToPointer(pair.Key, PointerPhase.Ended, Vector2.zero, pair.Value));
                    _endedIds.Add(pair.Key);
                }
            }

            foreach (int id in _endedIds)
                _tracked.Remove(id);
        }

        /// <summary>Forgets every pointer without reporting it as ended.</summary>
        public void Clear() => _tracked.Clear();

        private static TrackedPointer ToPointer(int id, PointerPhase phase, Vector2 delta, Tracked tracked)
        {
            return new TrackedPointer(id, phase, tracked.Position, delta, tracked.StartPosition, tracked.StartTime,
                tracked.MaxDistance, tracked.WasMultiTouch, tracked.StartedOverUI);
        }
    }
}
