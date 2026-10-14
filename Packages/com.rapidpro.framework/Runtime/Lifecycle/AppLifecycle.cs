using R3;
using RapidPro.Save;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RapidPro.Lifecycle
{
    /// <summary>
    /// Watches the app going to and from the background, and the Back button.
    /// Put it in the Bootstrap scene and connect it in your <c>GameInstaller</c>.
    /// </summary>
    /// <remarks>
    /// When the app goes to the background it pauses for <see cref="PauseReasons.Background"/>,
    /// flushes the save, then fires <see cref="WentToBackground"/>.
    /// When it returns it fires <see cref="ReturnedFromBackground"/>, and resumes right away if
    /// "Resume On Return" is ticked. Untick it once a "tap to continue" overlay calls
    /// <c>Resume(PauseReasons.Background)</c> itself.
    /// <code>
    /// [SerializeField] private AppLifecycle _lifecycle;
    ///
    /// _lifecycle.Connect(pause, save);
    /// Services.Register&lt;IAppLifecycle&gt;(_lifecycle);
    /// </code>
    /// </remarks>
    public class AppLifecycle : MonoBehaviour, IAppLifecycle
    {
        [Tooltip("Resume as soon as the app returns from the background. Untick once a 'tap to continue' overlay resumes the game instead.")]
        [SerializeField] private bool _resumeOnReturn = true;

        private readonly Subject<Unit> _wentToBackground = new Subject<Unit>();
        private readonly Subject<Unit> _returnedFromBackground = new Subject<Unit>();
        private readonly Subject<Unit> _backPressed = new Subject<Unit>();

        private IPauseService _pause;
        private ISaveService _save;
        private bool _isInBackground;

        /// <inheritdoc />
        public Observable<Unit> WentToBackground => _wentToBackground;

        /// <inheritdoc />
        public Observable<Unit> ReturnedFromBackground => _returnedFromBackground;

        /// <inheritdoc />
        public Observable<Unit> BackPressed => _backPressed;

        /// <summary>Lets tests switch "Resume On Return" without the Inspector.</summary>
        internal bool ResumeOnReturn
        {
            set => _resumeOnReturn = value;
        }

        /// <summary>
        /// Gives the lifecycle the services it pauses and saves. Call it once from your <c>GameInstaller</c>.
        /// </summary>
        public void Connect(IPauseService pause, ISaveService save)
        {
            _pause = pause;
            _save = save;
        }

        /// <summary>Called by Unity, and by tests, when the app goes to or returns from the background.</summary>
        internal void HandleApplicationPause(bool goingToBackground)
        {
            if (goingToBackground == _isInBackground)
                return;

            _isInBackground = goingToBackground;

            if (goingToBackground)
            {
                _pause?.Pause(PauseReasons.Background);
                _save?.Flush();
                _wentToBackground.OnNext(Unit.Default);
            }
            else
            {
                if (_resumeOnReturn)
                    _pause?.Resume(PauseReasons.Background);

                _returnedFromBackground.OnNext(Unit.Default);
            }
        }

        /// <summary>Called by <see cref="Update"/>, and by tests, when Back is pressed.</summary>
        internal void HandleBackPressed()
        {
            _backPressed.OnNext(Unit.Default);
        }

        // Mobile sends this when the app is sent to the background. Focus changes are ignored on
        // purpose, so clicking another window in the Editor does not pause the game.
        private void OnApplicationPause(bool pauseStatus)
        {
            HandleApplicationPause(pauseStatus);
        }

        private void Update()
        {
            if (WasBackPressedThisFrame())
                HandleBackPressed();
        }

        private void OnDestroy()
        {
            _wentToBackground.Dispose();
            _returnedFromBackground.Dispose();
            _backPressed.Dispose();
        }

        // On Android the Back button arrives as the Escape key.
        private static bool WasBackPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
