using System.Runtime.CompilerServices;

// Lets the edit-mode tests read the SFX pool and BGM channels without making them public.
[assembly: InternalsVisibleTo("RapidPro.Audio.Tests")]
