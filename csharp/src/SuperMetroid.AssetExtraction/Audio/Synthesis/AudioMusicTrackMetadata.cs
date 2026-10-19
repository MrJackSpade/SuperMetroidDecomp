using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One numbered music track's top-level phrase-flow program.</summary>
public sealed record AudioMusicTrackMetadata(
    int Track,
    string Id,
    int Address,
    int ByteCapacity,
    IReadOnlyList<AudioMusicTrackInstructionMetadata> Instructions)
{
    /// <summary>The track's audio-RAM address, 0 through 65535.</summary>
    public int Address { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Address);
}
