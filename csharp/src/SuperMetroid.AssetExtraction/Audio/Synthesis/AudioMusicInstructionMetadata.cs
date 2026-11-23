using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// One channel command (#2548): what it is, decoded once, and its bytes. Timing contains the optional
/// length/articulation prefix; Arguments contains effect operands. Opcode is retained for exact lossless recompilation,
/// and names an effect command's effect.
/// </summary>
/// <param name="Kind">What the command is.</param>
/// <param name="Opcode">The command's opcode byte.</param>
/// <param name="Timing">The timing prefix bytes, none, one or two.</param>
/// <param name="Arguments">The effect operand bytes; none for any other command.</param>
public sealed partial record AudioMusicInstructionMetadata(
    AudioMusicInstructionKind Kind,
    int Opcode,
    IReadOnlyList<byte> Timing,
    IReadOnlyList<byte> Arguments)
{
    /// <summary>The command's opcode byte, 0 through 255.</summary>
    public int Opcode { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Opcode);
}
