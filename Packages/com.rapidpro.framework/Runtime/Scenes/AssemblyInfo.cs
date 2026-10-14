using System.Runtime.CompilerServices;

// Lets the edit-mode tests configure settings, and the Editor hook share the "play from any scene" rules.
[assembly: InternalsVisibleTo("RapidPro.Scenes.Tests")]
[assembly: InternalsVisibleTo("RapidPro.Scenes.Editor")]
