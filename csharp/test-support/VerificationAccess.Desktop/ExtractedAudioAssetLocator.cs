using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Desktop;

/// <summary>Finds extracted audio beside the working or executable directory for verification hosts.</summary>
internal static class ExtractedAudioAssetLocator
{
    /// <summary>Walks upward from the working and executable directories to find an audio manifest.</summary>
    internal static string FindAudioDirectory()
    {
        foreach (string start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            for (DirectoryInfo? directory = new(Path.GetFullPath(start)); directory is not null; directory = directory.Parent)
            {
                // The repository verification installation (csharp/.cache), then a hand extraction.
                foreach (string nested in new[]
                {
                    Path.Combine(directory.FullName, "csharp", ".cache", "verification-installed-content", "game", "audio"),
                    Path.Combine(directory.FullName, "standalone-assets", "audio"),
                })
                {
                    if (File.Exists(Path.Combine(nested, SuperMetroid.AssetExtraction.Audio.Synthesis.ExtractedAudioAssetCatalog.ManifestFileName)))
                        return nested;
                }
                // Published builds copy the contents under an adjacent audio directory.
                string adjacent = Path.Combine(directory.FullName, "audio");
                if (File.Exists(Path.Combine(adjacent, SuperMetroid.AssetExtraction.Audio.Synthesis.ExtractedAudioAssetCatalog.ManifestFileName)))
                    return adjacent;
            }
        }
        throw new DirectoryNotFoundException(
            "Could not find standalone-assets/audio/audio-manifest.json from the working or executable directory. " +
            "Run SuperMetroid.DebugRunner assets audio standalone-assets/raw standalone-assets/audio first.");
    }
}
