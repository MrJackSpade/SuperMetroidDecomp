using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.AssetExtraction.SpcExtractionData;

namespace SuperMetroid.AssetExtraction;

/// <summary>Development-tool members of <see cref="SpcAudioAssetExtractor"/>; never linked by player hosts.</summary>
internal static class SpcAudioAssetExtractorTooling
{
    /// <summary>Extracts all required assets from the repository's private raw directory.</summary>
    public static AudioAssetManifest Extract(string rawDirectory, string audioDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(audioDirectory);
        rawDirectory = Path.GetFullPath(rawDirectory);
        audioDirectory = Path.GetFullPath(audioDirectory);
        if (!Directory.Exists(rawDirectory))
            throw new DirectoryNotFoundException($"Raw asset directory '{rawDirectory}' does not exist.");

        return SpcAudioAssetExtractor.Extract(definition => File.ReadAllBytes(Path.Combine(rawDirectory, definition.Name + ".bin")), audioDirectory);
    }
}
