namespace SuperMetroid.Core.Audio;

/// <summary>Identifies one sound effect's clip: the bank context it plays in, its library and its command.</summary>
/// <param name="Context">The audio bank loaded when it plays, or null for a clip shared by every bank.</param>
/// <param name="Library">The sound-effect library.</param>
/// <param name="Command">The library command that requests it.</param>
internal readonly record struct SoundEffectClipKey(AudioBank? Context, SoundEffectLibrary Library, int Command);
