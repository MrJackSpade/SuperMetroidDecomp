namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Serializable root of the inspectable extracted-audio catalog.</summary>
public sealed partial record AudioAssetManifest(
    int FormatVersion,
    IReadOnlyList<AudioUploadManifestEntry> Uploads,
    IReadOnlyList<AudioCanonicalSampleMetadata> CanonicalSamples,
    IReadOnlyList<AudioBankMetadata> Banks,
    IReadOnlyList<AudioSoundProgramMetadata> SoundPrograms,
    IReadOnlyList<AudioSoundLibraryMetadata> SoundLibraries)
{
    /// <summary>
    /// Accepted manifest schema version: canonical PCM aliases and decoded, slot-stable music/SFX programs. Version 6 (#2548)
    /// names each phrase and channel program by its place among its bank's instead of its slot's address, has each track
    /// play a phrase by its identity, repeats a track from the place of one of its own instructions instead of an address,
    /// and holds each track instruction as its own closed kind. Version 7 (#2556) names each track by the song it plays and
    /// installs no track for a slot that plays no song.
    /// </summary>
    public const int CurrentFormatVersion = 7;
}
