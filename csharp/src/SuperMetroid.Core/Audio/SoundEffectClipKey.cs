namespace SuperMetroid.Core.Audio;

/// <summary>Identifies one sound effect's clip: the data set it plays in and the sound.</summary>
/// <param name="Context">The music data set loaded when it plays, or null for only the shared sounds.</param>
/// <param name="Sound">The sound effect.</param>
internal readonly record struct SoundEffectClipKey(MusicDataSet? Context, SoundEffectId Sound);
