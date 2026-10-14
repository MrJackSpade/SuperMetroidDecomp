namespace SuperMetroid.Core.Audio;

/// <summary>Eight-channel routing table selected by a top-level track instruction.</summary>
public sealed record AudioMusicPhraseMetadata(
    string Id,
    ushort Address,
    IReadOnlyList<string?> ChannelPrograms);
