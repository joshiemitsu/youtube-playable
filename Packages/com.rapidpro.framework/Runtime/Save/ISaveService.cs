namespace RapidPro.Save
{
    /// <summary>
    /// Stores small pieces of player data (current level, volumes, settings) under string keys.
    /// </summary>
    /// <remarks>
    /// <see cref="Save{T}"/> only changes memory, so it is cheap to call often (e.g. from a slider).
    /// <see cref="Flush"/> writes to disk. Call it after important moments such as finishing a level;
    /// it also runs automatically when the app quits, and when it goes to the background once
    /// <c>AppLifecycle</c> is connected.
    /// <para>
    /// Values can be <c>int</c>, <c>float</c>, <c>bool</c>, <c>string</c>, or any <c>[Serializable]</c> class or struct.
    /// Name keys by feature, e.g. <c>"audio.musicVolume"</c> or <c>"progress.level"</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var save = Services.Get&lt;ISaveService&gt;();
    ///
    /// int level = save.Load("progress.level", 1);
    /// save.Save("progress.level", level + 1);
    /// save.Flush();
    /// </code>
    /// </example>
    public interface ISaveService
    {
        /// <summary>Returns the value saved under <paramref name="key"/>, or <paramref name="fallback"/> when there is none.</summary>
        T Load<T>(string key, T fallback);

        /// <summary>Stores <paramref name="value"/> under <paramref name="key"/> in memory. Call <see cref="Flush"/> to write it to disk.</summary>
        void Save<T>(string key, T value);

        /// <summary>True when a value is saved under <paramref name="key"/>.</summary>
        bool HasKey(string key);

        /// <summary>Removes the value saved under <paramref name="key"/>.</summary>
        void Delete(string key);

        /// <summary>Removes every value and the save file. Used by "Reset save" in the debug panel.</summary>
        void DeleteAll();

        /// <summary>Writes unsaved changes to disk. Does nothing when nothing changed.</summary>
        void Flush();
    }
}
