namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Seeds the persistent editable-audio directory from a validated stock installation.
/// The resulting full catalog is intentionally outside <c>game/</c>, so repair and update
/// transactions can replace stock content without erasing user WAV or manifest edits.
/// </summary>
public static class AudioAssetOverrideInstaller
{
    public static string Initialize(GameInstallation installation)
    {
        ArgumentNullException.ThrowIfNull(installation);
        _ = installation.LoadAudio();

        string source = Path.GetFullPath(installation.AudioDirectory);
        string destination = Path.GetFullPath(installation.AudioOverrideDirectory);
        if (Directory.Exists(destination))
        {
            throw new IOException(
                $"Audio override directory '{destination}' already exists; refusing to overwrite user edits.");
        }

        string parent = Path.GetDirectoryName(destination)
            ?? throw new InvalidDataException($"Audio override path '{destination}' has no parent directory.");
        Directory.CreateDirectory(parent);
        string staging = Path.Combine(parent, $".audio-override-{Guid.NewGuid():N}");
        try
        {
            CopyDirectory(source, staging);
            // Validate the isolated copy before it can become the selected override.
            _ = SuperMetroid.Core.Audio.ExtractedAudioAssetCatalog.Load(source, staging);
            Directory.Move(staging, destination);
            return destination;
        }
        finally
        {
            if (Directory.Exists(staging))
                Directory.Delete(staging, recursive: true);
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(
                destination,
                Path.GetRelativePath(source, directory)));
        }
        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
        }
    }
}
