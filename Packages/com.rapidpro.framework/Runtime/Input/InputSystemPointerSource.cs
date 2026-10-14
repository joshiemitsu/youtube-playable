using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RapidPro.Input
{
    /// <summary>
    /// Reads touches, the left mouse button and the scroll wheel. Touch on phones and tablets;
    /// mouse on desktop, WebGL and in the Editor. Uses the Input System when it is enabled,
    /// otherwise Unity's older Input class.
    /// </summary>
    public class InputSystemPointerSource : IPointerSource
    {
        /// <inheritdoc />
        public Vector2 ScreenSize => new Vector2(Screen.width, Screen.height);

        /// <inheritdoc />
        public void ReadPointers(List<RawPointer> pointers)
        {
#if ENABLE_INPUT_SYSTEM
            // "Pressed this frame" also counts, so a very quick tap that goes down and up within one
            // frame is still seen: it is down this frame and released the next.
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (var touch in touchscreen.touches)
                {
                    if (touch.press.isPressed || touch.press.wasPressedThisFrame)
                        pointers.Add(new RawPointer(touch.touchId.ReadValue(), touch.position.ReadValue()));
                }
            }

            var mouse = Mouse.current;
            if (pointers.Count == 0 && mouse != null && (mouse.leftButton.isPressed || mouse.leftButton.wasPressedThisFrame))
                pointers.Add(new RawPointer(RawPointer.MouseId, mouse.position.ReadValue()));
#else
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                    pointers.Add(new RawPointer(touch.fingerId, touch.position));
            }

            if (pointers.Count == 0 && UnityEngine.Input.GetMouseButton(0))
                pointers.Add(new RawPointer(RawPointer.MouseId, UnityEngine.Input.mousePosition));
#endif
        }

        /// <inheritdoc />
        public float ReadScroll()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null)
                return 0f;

            // The Input System reports about 120 per wheel notch on Windows; normalise to 1 per notch.
            return mouse.scroll.ReadValue().y / 120f;
#else
            return UnityEngine.Input.mouseScrollDelta.y;
#endif
        }

        /// <inheritdoc />
        public Vector2 ReadCursorPosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : ScreenSize * 0.5f;
#else
            return UnityEngine.Input.mousePosition;
#endif
        }
    }
}
