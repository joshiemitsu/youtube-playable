using System;
using System.Collections.Generic;

namespace RapidPro.Core
{
    /// <summary>
    /// A simple service locator. Register each service by its interface type,
    /// then look it up from anywhere with <see cref="Get{T}"/> or <see cref="TryGet{T}"/>.
    /// </summary>
    /// <remarks>
    /// Register services by hand in a <see cref="GameInstaller"/> so the whole setup reads top to bottom.
    /// </remarks>
    public static class Services
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service under type <typeparamref name="T"/>. Use the interface type,
        /// e.g. <c>Services.Register&lt;IScoreService&gt;(new ScoreService());</c>.
        /// </summary>
        /// <param name="service">The service instance. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a service of type <typeparamref name="T"/> is already registered.</exception>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service), $"Cannot register a null {typeof(T).Name}.");

            if (_services.ContainsKey(typeof(T)))
                throw new InvalidOperationException(
                    $"A service of type {typeof(T).Name} is already registered. Call Unregister<{typeof(T).Name}>() first to replace it.");

            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Returns the service registered under type <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no service of type <typeparamref name="T"/> is registered.</exception>
        public static T Get<T>() where T : class
        {
            if (TryGet(out T service))
                return service;

            throw new InvalidOperationException(
                $"No service of type {typeof(T).Name} is registered. Register it in your GameInstaller.");
        }

        /// <summary>
        /// Tries to find the service registered under type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="service">The service, or null when it is not registered.</param>
        /// <returns>True when the service is registered.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var found))
            {
                service = (T)found;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Removes the service registered under type <typeparamref name="T"/>. Does nothing when it is not registered.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        /// <summary>
        /// Removes every registered service.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
