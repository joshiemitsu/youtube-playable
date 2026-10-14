using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace RapidPro.Audio.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="AudioManager"/> and <see cref="AudioCue"/>.
    /// Clips are silent and 0.1 seconds long, so nothing is heard.
    /// </summary>
    public class AudioManagerTests
    {
        private GameObject _gameObject;
        private AudioManager _audio;
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("Audio Test");
            _audio = _gameObject.AddComponent<AudioManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
            foreach (var obj in _created)
                Object.DestroyImmediate(obj);
            _created.Clear();
        }

        [Test]
        public void Volumes_StartAtFull()
        {
            Assert.AreEqual(1f, _audio.MusicVolume.CurrentValue);
            Assert.AreEqual(1f, _audio.SfxVolume.CurrentValue);
        }

        [Test]
        public void SetVolumes_ClampBetweenZeroAndOne()
        {
            _audio.SetMusicVolume(-0.5f);
            _audio.SetSfxVolume(3f);
            Assert.AreEqual(0f, _audio.MusicVolume.CurrentValue);
            Assert.AreEqual(1f, _audio.SfxVolume.CurrentValue);

            _audio.SetMusicVolume(0.4f);
            Assert.AreEqual(0.4f, _audio.MusicVolume.CurrentValue, 0.0001f);
        }

        [Test]
        public void SetMusicVolume_ScalesPlayingMusic()
        {
            _audio.PlayBgm(CreateCue(0f, CreateClip("Music")), fade: 0f);

            _audio.SetMusicVolume(0.5f);

            Assert.AreEqual(0.5f, _audio.ActiveBgm.Source.volume, 0.0001f);
        }

        [Test]
        public void PlaySfx_WithinMinGap_PlaysOnlyOnce()
        {
            var cue = CreateCue(1f, CreateClip("Tap"));

            _audio.PlaySfx(cue);
            _audio.PlaySfx(cue);

            Assert.AreEqual(1, _audio.SfxPool.Count);
        }

        [Test]
        public void PlaySfx_WithoutMinGap_PlaysEachTime()
        {
            var cue = CreateCue(0f, CreateClip("Tap"));

            _audio.PlaySfx(cue);
            _audio.PlaySfx(cue);

            Assert.AreEqual(2, _audio.SfxPool.Count);
        }

        [Test]
        public void PlaySfx_ManyAtOnce_NeverUsesMoreThanEightSources()
        {
            var cue = CreateCue(0f, CreateClip("Tap"));

            for (int i = 0; i < 12; i++)
                _audio.PlaySfx(cue, Vector3.one * i);

            Assert.AreEqual(SfxSourcePool.MaxSources, _audio.SfxPool.Count);
        }

        [Test]
        public void SfxPool_FinishedSource_IsReused()
        {
            var pool = new SfxSourcePool(_gameObject.transform);

            var first = pool.Get(now: 0f, duration: 1f);
            var second = pool.Get(now: 5f, duration: 1f);

            Assert.AreSame(first, second);
            Assert.AreEqual(1, pool.Count);
        }

        [Test]
        public void Cue_PickClip_ReturnsClipFromItsList()
        {
            var clips = new[] { CreateClip("A"), CreateClip("B"), CreateClip("C") };
            var cue = CreateCue(0f, clips);

            for (int i = 0; i < 20; i++)
                CollectionAssert.Contains(clips, cue.PickClip());

            Assert.IsNull(CreateCue(0f).PickClip());
        }

        [Test]
        public void PlayBgm_NewCue_SwitchesToTheOtherChannel()
        {
            var menuMusic = CreateCue(0f, CreateClip("Menu"));
            var gameMusic = CreateCue(0f, CreateClip("Game"));

            _audio.PlayBgm(menuMusic, fade: 0f);
            var first = _audio.ActiveBgm;
            _audio.PlayBgm(gameMusic, fade: 0f);

            Assert.AreNotSame(first, _audio.ActiveBgm);
            Assert.AreSame(gameMusic, _audio.ActiveBgm.Cue);
            Assert.IsNull(first.Cue);
        }

        [Test]
        public void PlayBgm_SameCue_KeepsPlaying()
        {
            var music = CreateCue(0f, CreateClip("Menu"));

            _audio.PlayBgm(music, fade: 0f);
            var first = _audio.ActiveBgm;
            _audio.PlayBgm(music, fade: 0f);

            Assert.AreSame(first, _audio.ActiveBgm);
        }

        [Test]
        public void StopBgm_ClearsCurrentMusic()
        {
            _audio.PlayBgm(CreateCue(0f, CreateClip("Menu")), fade: 0f);

            _audio.StopBgm(fade: 0f);

            Assert.IsNull(_audio.ActiveBgm.Cue);
            Assert.AreEqual(0f, _audio.ActiveBgm.Source.volume);
        }

        private AudioClip CreateClip(string clipName)
        {
            var clip = AudioClip.Create(clipName, 4410, 1, 44100, false);
            _created.Add(clip);
            return clip;
        }

        private AudioCue CreateCue(float minGap, params AudioClip[] clips)
        {
            var cue = ScriptableObject.CreateInstance<AudioCue>();
            cue.Configure(clips, volume: 1f, minGap: minGap);
            _created.Add(cue);
            return cue;
        }
    }
}
