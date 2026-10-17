using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One bounded authored channel/subroutine program.</summary>
public sealed record AudioMusicProgramMetadata(
    string Id,
    int Address,
    int ByteCapacity,
    IReadOnlyList<AudioMusicInstructionMetadata> Instructions)
{
    /// <summary>The program's audio-RAM address, 0 through 65535.</summary>
    public int Address { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Address);
}
