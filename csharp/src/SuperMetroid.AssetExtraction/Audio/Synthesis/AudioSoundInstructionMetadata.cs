namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One named operation and its documented byte operands.</summary>
public sealed record AudioSoundInstructionMetadata(
    string Operation,
    IReadOnlyList<byte> Arguments);
