using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Eight-channel routing table selected by a top-level track instruction.</summary>
public sealed record AudioMusicPhraseMetadata(
    string Id,
    int Address,
    IReadOnlyList<string?> ChannelPrograms)
{
    /// <summary>The phrase table's audio-RAM address, 0 through 65535.</summary>
    public int Address { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Address);
}
