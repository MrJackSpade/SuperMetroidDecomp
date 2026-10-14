namespace SuperMetroid.Core.Audio;

/// <summary>One bounded authored channel/subroutine program.</summary>
public sealed record AudioMusicProgramMetadata(
    string Id,
    ushort Address,
    int ByteCapacity,
    IReadOnlyList<AudioMusicInstructionMetadata> Instructions);
