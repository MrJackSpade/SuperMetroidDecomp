namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One decoded sound-effect operation and its documented byte operands (#2548).</summary>
/// <param name="Operation">The operation, decoded once from its bytes.</param>
/// <param name="Arguments">Its byte operands.</param>
public sealed partial record AudioSoundInstructionMetadata(
    AudioSoundOperation Operation,
    IReadOnlyList<byte> Arguments);
