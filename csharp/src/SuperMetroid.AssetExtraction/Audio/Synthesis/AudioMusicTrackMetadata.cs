namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One numbered music track's top-level phrase-flow program.</summary>
public sealed record AudioMusicTrackMetadata(
    int Track,
    string Id,
    ushort Address,
    int ByteCapacity,
    IReadOnlyList<AudioMusicTrackInstructionMetadata> Instructions);
