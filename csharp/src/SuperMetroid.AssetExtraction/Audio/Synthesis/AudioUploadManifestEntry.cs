namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Manifest identity and integrity information for one opaque upload stream.</summary>
public sealed partial record AudioUploadManifestEntry(
    int SnesAddress,
    string StreamFile);
