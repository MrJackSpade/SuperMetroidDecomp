using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>A bounded, address-stable resident SPC sound-effect channel program.</summary>
public sealed record AudioSoundProgramMetadata(
    string Id,
    int Address,
    int ByteCapacity,
    IReadOnlyList<AudioSoundInstructionMetadata> Instructions)
{
    /// <summary>The program's audio-RAM address, 0 through 65535.</summary>
    public int Address { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Address);
}
