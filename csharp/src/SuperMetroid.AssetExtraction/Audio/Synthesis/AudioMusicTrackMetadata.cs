using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// One song's top-level phrase-flow program in its bank (#2556): the song it is, named by its installed key; which slot of
/// the driver's track table plays it is the track-number codec's (<see cref="MusicTrackNumbers"/>), and a slot that plays no
/// song has no track.
/// </summary>
/// <param name="Song">The song the track plays.</param>
/// <param name="Address">The track's audio-RAM address.</param>
/// <param name="ByteCapacity">The bytes its slot holds.</param>
/// <param name="Instructions">Its phrase flow, its end last.</param>
public sealed partial record AudioMusicTrackMetadata(
    MusicTrack Song,
    int Address,
    int ByteCapacity,
    IReadOnlyList<AudioMusicTrackInstructionMetadata> Instructions)
{
    /// <summary>The track's audio-RAM address, 0 through 65535.</summary>
    public int Address { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Address);
}
