
namespace SuperMetroid.Core.Audio;

/// <summary>A music track within an audio bank.</summary>
/// <param name="Bank">The bank.</param>
/// <param name="Track">The track.</param>
public readonly record struct MusicTrackKey(AudioBank Bank, int Track);
