using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Decoded, human-readable metadata derived from one common-plus-music APU image.</summary>
public sealed record AudioBankMetadata(
    string Name,
    int DataIndex,
    int SnesAddress,
    IReadOnlyList<ushort> TrackPointers,
    IReadOnlyList<AudioMusicTrackMetadata> MusicTracks,
    IReadOnlyList<AudioMusicPhraseMetadata> MusicPhrases,
    IReadOnlyList<AudioMusicProgramMetadata> MusicPrograms,
    IReadOnlyList<AudioInstrumentMetadata> Instruments,
    IReadOnlyList<AudioSampleMetadata> Samples)
{
    /// <summary>The bank's audio data index, 0 through 255.</summary>
    public int DataIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, DataIndex);
}
