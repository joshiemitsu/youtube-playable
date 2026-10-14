using System;

namespace RapidPro.Save
{
    /// <summary>
    /// Wraps a single value so <c>JsonUtility</c> can write it.
    /// <c>JsonUtility</c> only writes objects, not plain <c>int</c> or <c>string</c> values.
    /// </summary>
    [Serializable]
    internal class SaveValue<T>
    {
        public T Value;
    }
}
