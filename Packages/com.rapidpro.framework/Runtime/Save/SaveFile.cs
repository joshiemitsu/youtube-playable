using System;
using System.Collections.Generic;

namespace RapidPro.Save
{
    /// <summary>
    /// The shape of the save file on disk: a list of key and JSON value pairs.
    /// <c>JsonUtility</c> cannot write dictionaries, so the service converts to and from this list.
    /// </summary>
    [Serializable]
    internal class SaveFile
    {
        public List<SaveEntry> Entries = new List<SaveEntry>();
    }
}
