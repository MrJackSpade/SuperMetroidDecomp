namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>A sound library configuration's voice count and priority.</summary>
/// <param name="Voices">The number of voices the library sets up.</param>
/// <param name="Priority">The library's priority.</param>
internal readonly record struct SoundLibrarySetup(int Voices, int Priority);
