namespace SuperMetroid.Core.Audio;

/// <summary>Identifies one sound effect's clip: the data set it plays in, its library and its command.</summary>
/// <param name="Context">The music data set loaded when it plays, or null for only the shared sounds.</param>
/// <param name="Library">The sound-effect library.</param>
/// <param name="Command">The library command that requests it.</param>
internal readonly record struct SoundEffectClipKey(MusicDataSet? Context, SoundEffectLibrary Library, int Command);
