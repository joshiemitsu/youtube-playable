using System;
using UnityEngine;

namespace RapidPro.Display
{
    /// <summary>
    /// The screen size and safe area at one moment. Fitters compare the current snapshot with the
    /// last one every frame and refit only when it changed: a rotated phone, a resized browser
    /// window in WebGL, or a resized window on desktop.
    /// </summary>
    public readonly struct ScreenSnapshot : IEquatable<ScreenSnapshot>
    {
        /// <summary>Screen width in pixels.</summary>
        public readonly int Width;

        /// <summary>Screen height in pixels.</summary>
        public readonly int Height;

        /// <summary>The part of the screen not covered by notches or rounded corners, in pixels.</summary>
        public readonly Rect SafeArea;

        /// <summary>Creates a snapshot from explicit values. Used by tests and tools.</summary>
        public ScreenSnapshot(int width, int height, Rect safeArea)
        {
            Width = width;
            Height = height;
            SafeArea = safeArea;
        }

        /// <summary>The screen right now.</summary>
        public static ScreenSnapshot Current => new ScreenSnapshot(Screen.width, Screen.height, Screen.safeArea);

        /// <summary>True when the screen is taller than it is wide.</summary>
        public bool IsPortrait => Height >= Width;

        /// <summary>The safe area as 0 to 1 fractions of the screen, ready for viewports and anchors.</summary>
        public Rect NormalizedSafeArea
        {
            get
            {
                if (Width <= 0 || Height <= 0)
                    return new Rect(0f, 0f, 1f, 1f);

                return new Rect(SafeArea.x / Width, SafeArea.y / Height, SafeArea.width / Width, SafeArea.height / Height);
            }
        }

        /// <inheritdoc />
        public bool Equals(ScreenSnapshot other) => Width == other.Width && Height == other.Height && SafeArea == other.SafeArea;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ScreenSnapshot other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Width, Height, SafeArea);

        /// <summary>True when both snapshots describe the same screen.</summary>
        public static bool operator ==(ScreenSnapshot left, ScreenSnapshot right) => left.Equals(right);

        /// <summary>True when the screen changed between the two snapshots.</summary>
        public static bool operator !=(ScreenSnapshot left, ScreenSnapshot right) => !left.Equals(right);
    }
}
