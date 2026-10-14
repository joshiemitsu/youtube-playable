using System.Collections.Generic;
using R3;
using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// Fires drag events once a finger moves past the drag threshold, and <see cref="Flicked"/> when a
    /// quick, fast move is released. A flick also ends its drag, so both <see cref="DragEnded"/> and
    /// <see cref="Flicked"/> fire for it.
    /// </summary>
    public class DragRecognizer : IGestureRecognizer
    {
        private readonly Subject<DragEvent> _dragStarted = new Subject<DragEvent>();
        private readonly Subject<DragEvent> _dragged = new Subject<DragEvent>();
        private readonly Subject<DragEvent> _dragEnded = new Subject<DragEvent>();
        private readonly Subject<FlickEvent> _flicked = new Subject<FlickEvent>();
        private readonly HashSet<int> _dragging = new HashSet<int>();

        /// <summary>A finger moved past the drag threshold.</summary>
        public Observable<DragEvent> DragStarted => _dragStarted;

        /// <summary>A dragging finger moved.</summary>
        public Observable<DragEvent> Dragged => _dragged;

        /// <summary>A dragging finger was lifted, or a second finger turned it into a pinch.</summary>
        public Observable<DragEvent> DragEnded => _dragEnded;

        /// <summary>A quick swipe was released while still moving fast.</summary>
        public Observable<FlickEvent> Flicked => _flicked;

        /// <inheritdoc />
        public void Process(PointerFrame frame, InputConfig config)
        {
            float dragThreshold = frame.ToPixels(config.DragThreshold);

            foreach (var pointer in frame.Pointers)
            {
                bool isDragging = _dragging.Contains(pointer.Id);

                if (!PointerFilter.IsSingleFinger(pointer, config))
                {
                    if (isDragging)
                        EndDrag(pointer);
                    continue;
                }

                if (pointer.Phase == PointerPhase.Ended)
                {
                    if (isDragging)
                        EndDrag(pointer);

                    TryFlick(pointer, frame, config, dragThreshold);
                    continue;
                }

                if (!isDragging && pointer.MaxDistance > dragThreshold)
                {
                    _dragging.Add(pointer.Id);
                    _dragStarted.OnNext(new DragEvent(pointer.StartPosition, pointer.Position, pointer.Position - pointer.StartPosition));
                }
                else if (isDragging && pointer.Phase == PointerPhase.Moved)
                {
                    _dragged.OnNext(new DragEvent(pointer.StartPosition, pointer.Position, pointer.Delta));
                }
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            _dragging.Clear();
        }

        private void EndDrag(TrackedPointer pointer)
        {
            _dragging.Remove(pointer.Id);
            _dragEnded.OnNext(new DragEvent(pointer.StartPosition, pointer.Position, Vector2.zero));
        }

        private void TryFlick(TrackedPointer pointer, PointerFrame frame, InputConfig config, float dragThreshold)
        {
            float duration = frame.Time - pointer.StartTime;
            if (duration <= 0f || duration > config.FlickMaxTime || pointer.MaxDistance <= dragThreshold)
                return;

            var movement = pointer.Position - pointer.StartPosition;
            var velocity = movement / duration;
            if (velocity.magnitude < frame.ToPixels(config.FlickMinSpeed))
                return;

            _flicked.OnNext(new FlickEvent(pointer.StartPosition, pointer.Position, velocity, ToDirection(movement)));
        }

        private static FlickDirection ToDirection(Vector2 movement)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                return movement.x > 0f ? FlickDirection.Right : FlickDirection.Left;

            return movement.y > 0f ? FlickDirection.Up : FlickDirection.Down;
        }
    }
}
