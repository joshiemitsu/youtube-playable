using System;

namespace RapidPro.Save
{
    /// <summary>One saved value: its key and the value as JSON.</summary>
    [Serializable]
    internal class SaveEntry
    {
        public string Key;
        public string Json;
    }
}
