namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Serializable root of the inspectable extracted-audio catalog.</summary>
public sealed record AudioAssetManifest(
    int FormatVersion,
    IReadOnlyList<AudioUploadManifestEntry> Uploads,
    IReadOnlyList<AudioCanonicalSampleMetadata> CanonicalSamples,
    IReadOnlyList<AudioBankMetadata> Banks,
    IReadOnlyList<AudioSoundProgramMetadata> SoundPrograms,
    IReadOnlyList<AudioSoundLibraryMetadata> SoundLibraries)
{
    /// <summary>Accepted manifest schema version 4, including canonical PCM aliases and decoded, address-stable music/SFX programs.</summary>
    public const int CurrentFormatVersion = 5;
}
