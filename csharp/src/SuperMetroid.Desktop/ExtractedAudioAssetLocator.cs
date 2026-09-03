using SuperMetroid.Core.Audio;

namespace SuperMetroid.Desktop;

/// <summary>Finds the private extracted-audio catalog in a checkout or packaged build.</summary>
internal static class ExtractedAudioAssetLocator
{
    internal static string FindAudioDirectory()
    {
        foreach (string start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            for (DirectoryInfo? directory = new(Path.GetFullPath(start));
                directory is not null;
                directory = directory.Parent)
            {
                string nested = Path.Combine(directory.FullName, "standalone-assets", "audio");
                if (File.Exists(Path.Combine(nested, ExtractedAudioAssetCatalog.ManifestFileName)))
                    return nested;

                // Published builds copy the contents under an adjacent audio directory.
                string adjacent = Path.Combine(directory.FullName, "audio");
                if (File.Exists(Path.Combine(adjacent, ExtractedAudioAssetCatalog.ManifestFileName)))
                    return adjacent;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not find standalone-assets/audio/audio-manifest.json from the working or executable directory. " +
            "Run SuperMetroid.AssetExtractor audio standalone-assets/raw standalone-assets/audio first.");
    }
}
