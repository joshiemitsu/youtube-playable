using System.Collections.Generic;
using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// A pointer source driven from code instead of a device. Use it for tutorial "ghost hands",
    /// replaying recorded input, or automated play tests.
    /// </summary>
    /// <example>
    /// <code>
    /// var script = new ScriptedPointerSource();
    /// inputService.SetSource(script);
    ///
    /// script.Press(0, new Vector2(500, 900));   // finger 0 down
    /// await UniTask.Delay(100);
    /// script.Release(0);                         // lifted: a tap
    ///
    /// inputService.SetSource(new InputSystemPointerSource());   // back to real input
    /// </code>
    /// </example>
    public class ScriptedPointerSource : IPointerSource
    {
        private readonly Dictionary<int, Vector2> _down = new Dictionary<int, Vector2>();
        private float _scroll;
        private Vector2? _screenSize;

        /// <summary>Screen size in pixels. Defaults to the real screen.</summary>
        public Vector2 ScreenSize
        {
            get => _screenSize ?? new Vector2(Screen.width, Screen.height);
            set => _screenSize = value;
        }

        /// <summary>Where the scroll-wheel pinch is centred. Defaults to the middle of the screen.</summary>
        public Vector2 CursorPosition { get; set; } = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        /// <summary>Puts pointer <paramref name="id"/> down at <paramref name="position"/>, or moves it there if already down.</summary>
        public void Press(int id, Vector2 position) => _down[id] = position;

        /// <summary>Moves a pointer that is down.</summary>
        public void Move(int id, Vector2 position)
        {
            if (_down.ContainsKey(id))
                _down[id] = position;
        }

        /// <summary>Lifts pointer <paramref name="id"/>.</summary>
        public void Release(int id) => _down.Remove(id);

        /// <summary>Scrolls the wheel once on the next frame. Positive = up.</summary>
        public void Scroll(float notches) => _scroll += notches;

        /// <inheritdoc />
        public void ReadPointers(List<RawPointer> pointers)
        {
            foreach (var pair in _down)
                pointers.Add(new RawPointer(pair.Key, pair.Value));
        }

        /// <inheritdoc />
        public float ReadScroll()
        {
            float scroll = _scroll;
            _scroll = 0f;
            return scroll;
        }

        /// <inheritdoc />
        public Vector2 ReadCursorPosition() => CursorPosition;
    }
}
