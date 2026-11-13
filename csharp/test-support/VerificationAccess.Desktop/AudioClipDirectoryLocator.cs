namespace SuperMetroid.Desktop;

/// <summary>Finds installed audio clips beside the working or executable directory for verification hosts.</summary>
internal static class AudioClipDirectoryLocator
{
    /// <summary>Walks upward from the working and executable directories to find an audio directory holding the clip index.</summary>
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
                    if (File.Exists(Path.Combine(nested, SuperMetroid.AssetExtraction.Audio.InstalledAudioClips.ManifestFileName)))
                        return nested;
                }
                // Published builds copy the contents under an adjacent audio directory.
                string adjacent = Path.Combine(directory.FullName, "audio");
                if (File.Exists(Path.Combine(adjacent, SuperMetroid.AssetExtraction.Audio.InstalledAudioClips.ManifestFileName)))
                    return adjacent;
            }
        }
        throw new DirectoryNotFoundException(
            "Could not find an audio directory holding audio-clips.json (csharp/.cache/verification-installed-content/game/audio " +
            "or standalone-assets/audio) from the working or executable directory. " +
            "Run SuperMetroid.DebugRunner assets audio-rom \"Super Metroid.smc\" standalone-assets/audio first.");
    }
}
