namespace SuperMetroid.Core.Audio;

/// <summary>Decoded, human-readable metadata derived from one common-plus-music APU image.</summary>
public sealed record AudioBankMetadata(
    string Name,
    byte DataIndex,
    int SnesAddress,
    IReadOnlyList<ushort> TrackPointers,
    IReadOnlyList<AudioMusicTrackMetadata> MusicTracks,
    IReadOnlyList<AudioMusicPhraseMetadata> MusicPhrases,
    IReadOnlyList<AudioMusicProgramMetadata> MusicPrograms,
    IReadOnlyList<AudioInstrumentMetadata> Instruments,
    IReadOnlyList<AudioSampleMetadata> Samples);
