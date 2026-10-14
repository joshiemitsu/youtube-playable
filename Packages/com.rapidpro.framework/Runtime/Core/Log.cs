using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace RapidPro.Core
{
    /// <summary>
    /// Thin wrapper over <see cref="Debug"/> for all framework and game logging.
    /// </summary>
    /// <remarks>
    /// <see cref="Info"/> and <see cref="Warning"/> calls are removed by the compiler from release builds.
    /// They only compile in the Editor, in development builds, or when <c>RAPIDPRO_LOG</c> is defined.
    /// <see cref="Error"/> is always kept so real failures still reach the device log.
    /// </remarks>
    public static class Log
    {
        /// <summary>Logs an informational message. Stripped from release builds.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">Optional object to highlight in the Editor when the log is clicked.</param>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("RAPIDPRO_LOG")]
        public static void Info(string message, UnityEngine.Object context = null)
        {
            Debug.Log(message, context);
        }

        /// <summary>Logs a warning. Stripped from release builds.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">Optional object to highlight in the Editor when the log is clicked.</param>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("RAPIDPRO_LOG")]
        public static void Warning(string message, UnityEngine.Object context = null)
        {
            Debug.LogWarning(message, context);
        }

        /// <summary>Logs an error. Always kept, including in release builds.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">Optional object to highlight in the Editor when the log is clicked.</param>
        public static void Error(string message, UnityEngine.Object context = null)
        {
            Debug.LogError(message, context);
        }
    }
}
