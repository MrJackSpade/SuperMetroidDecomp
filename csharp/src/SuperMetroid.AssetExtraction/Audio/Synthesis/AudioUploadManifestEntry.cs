using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Manifest identity and integrity information for one opaque upload stream.</summary>
public sealed record AudioUploadManifestEntry(
    int SnesAddress,
    string StreamFile,
    string Sha256);
