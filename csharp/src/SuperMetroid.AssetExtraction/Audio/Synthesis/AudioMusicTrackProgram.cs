using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>An active music track's top-level phrase-flow program and the slot it fills.</summary>
/// <param name="Address">The program's audio-RAM address, 0 through 65535.</param>
/// <param name="ByteCapacity">The bytes its slot holds.</param>
/// <param name="Instructions">Its instructions in stream order, its end last.</param>
public sealed record AudioMusicTrackProgram(
    int Address,
    int ByteCapacity,
    IReadOnlyList<AudioMusicTrackInstructionMetadata> Instructions)
{
    /// <summary>The program's audio-RAM address, 0 through 65535.</summary>
    public int Address { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Address);
}
