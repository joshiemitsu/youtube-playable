using System;
using R3;
using RapidPro.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RapidPro.Scenes
{
    /// <summary>
    /// Fills an Image as the game loads. Put it in the Loading scene on the progress bar.
    /// </summary>
    public class LoadingProgressBar : MonoBehaviour
    {
        [Tooltip("An Image with Image Type set to Filled. Its Fill Amount follows the loading progress.")]
        [SerializeField] private Image _fill;

        private IDisposable _subscription;

        private void OnEnable()
        {
            if (_fill == null || !Services.TryGet(out ISceneFlow flow))
                return;

            _subscription = flow.Progress.Subscribe(progress => _fill.fillAmount = progress);
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
