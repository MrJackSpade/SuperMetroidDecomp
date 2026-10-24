
namespace SuperMetroid.Core.Audio;

/// <summary>A music track within its music data set.</summary>
/// <param name="AudioSet">The data set.</param>
/// <param name="Track">The track.</param>
public readonly record struct MusicTrackKey(MusicDataSet AudioSet, int Track);
