using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Development-tool members of <see cref="SpcAudioAssetExtractor"/>; never linked by player hosts.</summary>
internal static class SpcAudioAssetExtractorTooling
{
    /// <summary>Extracts the audio's import data, the catalog the clips are rendered from, from the repository's private raw directory into <paramref name="importDataDirectory"/>.</summary>
    public static AudioAssetManifest Extract(string rawDirectory, string importDataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(importDataDirectory);
        rawDirectory = Path.GetFullPath(rawDirectory);
        importDataDirectory = Path.GetFullPath(importDataDirectory);
        if (!Directory.Exists(rawDirectory))
            throw new DirectoryNotFoundException($"Raw asset directory '{rawDirectory}' does not exist.");

        return SpcAudioAssetExtractor.Extract(definition => File.ReadAllBytes(Path.Combine(rawDirectory, definition.Name + ".bin")), importDataDirectory);
    }
}
