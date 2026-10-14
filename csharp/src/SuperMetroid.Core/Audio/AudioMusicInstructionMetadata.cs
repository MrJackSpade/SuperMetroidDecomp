namespace SuperMetroid.Core.Audio;

/// <summary>
/// One channel command. Timing contains the optional length/articulation prefix; Arguments
/// contains effect operands. Opcode is retained for exact lossless recompilation.
/// </summary>
public sealed record AudioMusicInstructionMetadata(
    string Operation,
    byte Opcode,
    IReadOnlyList<byte> Timing,
    IReadOnlyList<byte> Arguments);
