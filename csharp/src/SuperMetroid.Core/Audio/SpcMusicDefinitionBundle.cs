namespace SuperMetroid.Core.Audio;

/// <summary>Tracks and the distinct phrase and channel programs reachable from them.</summary>
internal sealed record SpcMusicDefinitionBundle(
    IReadOnlyList<AudioMusicTrackMetadata> Tracks,
    IReadOnlyList<AudioMusicPhraseMetadata> Phrases,
    IReadOnlyList<AudioMusicProgramMetadata> Programs);
