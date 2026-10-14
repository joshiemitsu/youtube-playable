using System;
using System.Collections.Generic;
using System.IO;
using RapidPro.Core;
using UnityEngine;

namespace RapidPro.Save
{
    /// <summary>
    /// Default <see cref="ISaveService"/>. Keeps values in memory and writes them as one JSON file.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The file is read once, when the service is created.</item>
    /// <item>Writes go to a temporary file first and then replace the real one, so a crash mid-write
    /// cannot leave a half-written save.</item>
    /// <item>A file that cannot be read is logged as a warning and the game starts with empty data.</item>
    /// <item>Changes are flushed when the app quits, and by <c>AppLifecycle</c> (RapidPro.Lifecycle)
    /// when the app goes to the background.</item>
    /// </list>
    /// Create it first in your <c>GameInstaller</c>, before services that load from it:
    /// <code>
    /// var save = new SaveService();
    /// Services.Register&lt;ISaveService&gt;(save);
    /// </code>
    /// </remarks>
    public class SaveService : ISaveService, IDisposable
    {
        /// <summary>File name used when no path is given, inside <c>Application.persistentDataPath</c>.</summary>
        public const string DefaultFileName = "save.json";

        private readonly string _filePath;
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
        private bool _isDirty;

        /// <summary>
        /// Creates the service and reads the save file.
        /// </summary>
        /// <param name="filePath">Full path of the save file. Defaults to <c>persistentDataPath/save.json</c>.</param>
        public SaveService(string filePath = null)
        {
            _filePath = string.IsNullOrEmpty(filePath)
                ? Path.Combine(Application.persistentDataPath, DefaultFileName)
                : filePath;

            ReadFile();
            Application.quitting += Flush;
        }

        /// <summary>Full path of the save file.</summary>
        public string FilePath => _filePath;

        /// <inheritdoc />
        public T Load<T>(string key, T fallback)
        {
            if (!_values.TryGetValue(key, out var json))
                return fallback;

            try
            {
                return JsonUtility.FromJson<SaveValue<T>>(json).Value;
            }
            catch (Exception exception)
            {
                Log.Warning($"SaveService: could not read '{key}' as {typeof(T).Name}, using the fallback. {exception.Message}");
                return fallback;
            }
        }

        /// <inheritdoc />
        public void Save<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Save needs a key.", nameof(key));

            _values[key] = JsonUtility.ToJson(new SaveValue<T> { Value = value });
            _isDirty = true;
        }

        /// <inheritdoc />
        public bool HasKey(string key) => _values.ContainsKey(key);

        /// <inheritdoc />
        public void Delete(string key)
        {
            if (_values.Remove(key))
                _isDirty = true;
        }

        /// <inheritdoc />
        public void DeleteAll()
        {
            _values.Clear();
            _isDirty = false;

            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        /// <inheritdoc />
        public void Flush()
        {
            if (!_isDirty)
                return;

            var file = new SaveFile();
            foreach (var pair in _values)
                file.Entries.Add(new SaveEntry { Key = pair.Key, Json = pair.Value });

            string tempPath = _filePath + ".tmp";
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.WriteAllText(tempPath, JsonUtility.ToJson(file, prettyPrint: true));

            if (File.Exists(_filePath))
                File.Delete(_filePath);
            File.Move(tempPath, _filePath);

            _isDirty = false;
        }

        /// <summary>Flushes, then stops listening for the app quitting.</summary>
        public void Dispose()
        {
            Flush();
            Application.quitting -= Flush;
        }

        private void ReadFile()
        {
            if (!File.Exists(_filePath))
                return;

            try
            {
                var file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(_filePath));
                if (file?.Entries == null)
                    throw new InvalidDataException("The file has no entries list.");

                foreach (var entry in file.Entries)
                    _values[entry.Key] = entry.Json;
            }
            catch (Exception exception)
            {
                _values.Clear();
                Log.Warning($"SaveService: could not read the save file at '{_filePath}', starting with empty data. {exception.Message}");
            }
        }
    }
}
