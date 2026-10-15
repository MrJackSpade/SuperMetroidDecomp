namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>A bounded, address-stable resident SPC sound-effect channel program.</summary>
public sealed record AudioSoundProgramMetadata(
    string Id,
    ushort Address,
    int ByteCapacity,
    IReadOnlyList<AudioSoundInstructionMetadata> Instructions);
