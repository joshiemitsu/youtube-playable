using System.Collections.Generic;
using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// Where raw input comes from. It only has to say which pointers are down and where; the
    /// <see cref="InputService"/> works out presses, releases and gestures.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="InputSystemPointerSource"/> (touch, mouse and scroll wheel).
    /// Write your own to replay recorded input, drive a tutorial, or support a new device, then pass it
    /// to <see cref="InputService.SetSource"/>.
    /// </remarks>
    public interface IPointerSource
    {
        /// <summary>Size of the area the positions are in, normally the screen, in pixels.</summary>
        Vector2 ScreenSize { get; }

        /// <summary>Fills <paramref name="pointers"/> with every pointer that is down this frame. The list arrives empty.</summary>
        void ReadPointers(List<RawPointer> pointers);

        /// <summary>Scroll wheel movement this frame. Positive = scroll up. Used as pinch on desktop and WebGL.</summary>
        float ReadScroll();

        /// <summary>Position of the mouse cursor, used as the centre of a scroll-wheel pinch.</summary>
        Vector2 ReadCursorPosition();
    }
}
