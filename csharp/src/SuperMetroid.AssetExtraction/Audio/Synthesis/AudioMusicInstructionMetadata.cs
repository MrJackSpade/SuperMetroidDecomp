using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// One channel command. Timing contains the optional length/articulation prefix; Arguments
/// contains effect operands. Opcode is retained for exact lossless recompilation.
/// </summary>
public sealed record AudioMusicInstructionMetadata(
    string Operation,
    int Opcode,
    IReadOnlyList<byte> Timing,
    IReadOnlyList<byte> Arguments)
{
    /// <summary>The command's opcode byte, 0 through 255.</summary>
    public int Opcode { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Opcode);
}
