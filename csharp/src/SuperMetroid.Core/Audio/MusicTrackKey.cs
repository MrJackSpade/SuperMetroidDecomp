
namespace SuperMetroid.Core.Audio;

/// <summary>A song within its clip set, the key the clip library finds it by.</summary>
/// <param name="AudioSet">The music data set naming the clip set.</param>
/// <param name="Track">The song's track number within the data set.</param>
public readonly record struct MusicTrackKey(MusicDataSet AudioSet, int Track);
