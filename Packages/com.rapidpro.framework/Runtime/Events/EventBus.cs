using System;
using System.Collections.Generic;
using R3;

namespace RapidPro.Events
{
    /// <summary>
    /// Default <see cref="IEventBus"/>. Keeps one private R3 <see cref="Subject{T}"/> per event type.
    /// </summary>
    /// <remarks>
    /// Register it once in your <c>GameInstaller</c>:
    /// <c>Services.Register&lt;IEventBus&gt;(new EventBus());</c>
    /// </remarks>
    public class EventBus : IEventBus, IDisposable
    {
        private readonly Dictionary<Type, object> _subjects = new Dictionary<Type, object>();

        /// <inheritdoc />
        public void Publish<T>(T evt) where T : struct
        {
            if (_subjects.TryGetValue(typeof(T), out var subject))
                ((Subject<T>)subject).OnNext(evt);
        }

        /// <inheritdoc />
        public Observable<T> On<T>() where T : struct
        {
            if (!_subjects.TryGetValue(typeof(T), out var subject))
            {
                subject = new Subject<T>();
                _subjects[typeof(T)] = subject;
            }

            return ((Subject<T>)subject).AsObservable();
        }

        /// <summary>
        /// Completes every event stream and removes all subscribers.
        /// </summary>
        public void Dispose()
        {
            foreach (var subject in _subjects.Values)
                ((IDisposable)subject).Dispose();

            _subjects.Clear();
        }
    }
}
