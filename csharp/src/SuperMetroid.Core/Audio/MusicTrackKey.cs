
namespace SuperMetroid.Core.Audio;

/// <summary>A music track within an audio bank.</summary>
/// <param name="AudioSet">The bank.</param>
/// <param name="Track">The track.</param>
public readonly record struct MusicTrackKey(AudioBank AudioSet, int Track);
