using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RapidPro.Save.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="SaveService"/>. Each test uses its own file in the temp folder.
    /// </summary>
    public class SaveServiceTests
    {
        [Serializable]
        private class Progress
        {
            public int Level;
            public List<int> Stars = new List<int>();
        }

        private string _filePath;
        private SaveService _save;

        [SetUp]
        public void SetUp()
        {
            _filePath = Path.Combine(Application.temporaryCachePath, $"save-test-{Guid.NewGuid():N}.json");
            _save = new SaveService(_filePath);
        }

        [TearDown]
        public void TearDown()
        {
            _save.DeleteAll();
            _save.Dispose();
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        [Test]
        public void Load_MissingKey_ReturnsFallback()
        {
            Assert.AreEqual(7, _save.Load("missing", 7));
            Assert.IsFalse(_save.HasKey("missing"));
        }

        [Test]
        public void SaveThenLoad_ReturnsSameValues()
        {
            _save.Save("int", 42);
            _save.Save("float", 0.5f);
            _save.Save("bool", true);
            _save.Save("string", "hello");

            Assert.AreEqual(42, _save.Load("int", 0));
            Assert.AreEqual(0.5f, _save.Load("float", 0f));
            Assert.AreEqual(true, _save.Load("bool", false));
            Assert.AreEqual("hello", _save.Load("string", ""));
        }

        [Test]
        public void SaveThenLoad_SerializableClass()
        {
            _save.Save("progress", new Progress { Level = 12, Stars = new List<int> { 3, 2, 3 } });

            var loaded = _save.Load<Progress>("progress", null);

            Assert.AreEqual(12, loaded.Level);
            CollectionAssert.AreEqual(new[] { 3, 2, 3 }, loaded.Stars);
        }

        [Test]
        public void Flush_ThenNewService_LoadsValuesFromDisk()
        {
            _save.Save("progress.level", 5);

            _save.Flush();
            var reopened = new SaveService(_filePath);

            Assert.AreEqual(5, reopened.Load("progress.level", 0));
            reopened.Dispose();
        }

        [Test]
        public void Save_WithoutFlush_IsNotOnDisk()
        {
            _save.Save("progress.level", 5);

            Assert.IsFalse(File.Exists(_filePath));
        }

        [Test]
        public void Flush_LeavesNoTempFile()
        {
            _save.Save("progress.level", 5);

            _save.Flush();

            Assert.IsTrue(File.Exists(_filePath));
            Assert.IsFalse(File.Exists(_filePath + ".tmp"));
        }

        [Test]
        public void Delete_RemovesKey()
        {
            _save.Save("progress.level", 5);

            _save.Delete("progress.level");

            Assert.IsFalse(_save.HasKey("progress.level"));
        }

        [Test]
        public void DeleteAll_ClearsValuesAndFile()
        {
            _save.Save("progress.level", 5);
            _save.Flush();

            _save.DeleteAll();

            Assert.IsFalse(_save.HasKey("progress.level"));
            Assert.IsFalse(File.Exists(_filePath));
        }

        [Test]
        public void CorruptFile_WarnsAndStartsEmpty()
        {
            File.WriteAllText(_filePath, "this is not json");
            LogAssert.Expect(LogType.Warning, new Regex("could not read the save file"));

            var reopened = new SaveService(_filePath);

            Assert.IsFalse(reopened.HasKey("progress.level"));
            reopened.Dispose();
        }
    }
}
