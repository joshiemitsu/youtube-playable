using System.Collections.Generic;
using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// Everything a gesture recognizer needs for one frame: the pointers, the time, the screen size
    /// and the scroll wheel. Positions are in pixels; use <see cref="ToPixels"/> to turn the config's
    /// screen-relative distances into pixels.
    /// </summary>
    public class PointerFrame
    {
        private readonly List<TrackedPointer> _pointers = new List<TrackedPointer>();

        /// <summary>Every pointer that is down, went down, or was lifted this frame.</summary>
        public IReadOnlyList<TrackedPointer> Pointers => _pointers;

        /// <summary>Unscaled time in seconds, so gestures work while the game is paused.</summary>
        public float Time { get; private set; }

        /// <summary>Screen size in pixels.</summary>
        public Vector2 ScreenSize { get; private set; }

        /// <summary>Scroll wheel movement this frame, 1 per notch. Positive = up.</summary>
        public float Scroll { get; private set; }

        /// <summary>Mouse cursor position in pixels.</summary>
        public Vector2 CursorPosition { get; private set; }

        /// <summary>True when the mouse cursor is over a UI element.</summary>
        public bool CursorOverUI { get; private set; }

        /// <summary>How many pointers are down (not counting ones lifted this frame).</summary>
        public int DownCount { get; private set; }

        /// <summary>
        /// Turns a distance given as a fraction of the screen's shorter side (as in <see cref="InputConfig"/>)
        /// into pixels, so thresholds feel the same on every resolution.
        /// </summary>
        public float ToPixels(float screenFraction) => screenFraction * Mathf.Min(ScreenSize.x, ScreenSize.y);

        internal void Set(float time, Vector2 screenSize, float scroll, Vector2 cursorPosition, bool cursorOverUI)
        {
            Time = time;
            ScreenSize = screenSize;
            Scroll = scroll;
            CursorPosition = cursorPosition;
            CursorOverUI = cursorOverUI;
            _pointers.Clear();
            DownCount = 0;
        }

        internal void Add(TrackedPointer pointer)
        {
            _pointers.Add(pointer);
            if (pointer.IsDown)
                DownCount++;
        }
    }
}
