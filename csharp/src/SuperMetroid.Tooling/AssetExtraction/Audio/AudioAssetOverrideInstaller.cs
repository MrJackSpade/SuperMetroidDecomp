using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Creates and edits the persistent audio override (#1305 Q3): finished replacement clips kept in
/// <c>overrides/audio</c>, outside the replaceable game content, keyed by song and sound-effect
/// identity. Every change is staged in a copy and validated against the stock clips before it
/// replaces the override, so a failed edit leaves the previous override untouched.
/// </summary>
public static class AudioAssetOverrideInstaller
{
    /// <summary>Directory of replacement files inside the override.</summary>
    private const string ClipDirectory = "clips";

    /// <summary>Creates an empty override beside a validated stock installation.</summary>
    /// <returns>The override directory.</returns>
    /// <exception cref="IOException">An override already exists; user edits are never overwritten.</exception>
    /// <exception cref="InvalidDataException">The stock clips are invalid.</exception>
    public static string Initialize(GameInstallation installation)
    {
        Ensure.NotNull(installation);
        string destination = Path.GetFullPath(installation.AudioOverrideDirectory);
        if (Directory.Exists(destination))
        {
            throw new IOException(
                $"Audio override directory '{destination}' already exists; refusing to overwrite user edits.");
        }
        _ = InstalledAudioClips.Load(installation.AudioDirectory);
        Publish(installation, staging =>
            WriteManifest(staging, new AudioClipOverrideManifest(AudioClipOverrideManifest.CurrentFormatVersion, [])));
        return destination;
    }

    /// <summary>
    /// Installs <paramref name="wavPath"/> as the replacement for clip <paramref name="clipId"/>,
    /// replacing an earlier replacement of the same clip.
    /// </summary>
    /// <param name="installation">The installation whose override is edited; it must be initialized.</param>
    /// <param name="clipId">A song (<c>music-{data index:x2}-track-{nn}</c>) or sound effect (<c>sfx-{library}-{command:x2}</c>).</param>
    /// <param name="wavPath">An uncompressed 16-bit stereo WAV at the native 32,040 Hz.</param>
    /// <param name="loopStart">The frame the clip loops back to, or null when it plays once.</param>
    /// <returns>The installed replacement.</returns>
    /// <exception cref="DirectoryNotFoundException">The override has not been initialized.</exception>
    /// <exception cref="InvalidDataException">The identity is unknown, or the WAV's format or loop does not fit the clip.</exception>
    public static AudioClipOverride Replace(GameInstallation installation, string clipId, string wavPath, long? loopStart)
    {
        Ensure.NotNull(installation);
        ArgumentException.ThrowIfNullOrWhiteSpace(clipId);
        ArgumentException.ThrowIfNullOrWhiteSpace(wavPath);
        string current = Path.GetFullPath(installation.AudioOverrideDirectory);
        if (!Directory.Exists(current))
            throw new DirectoryNotFoundException($"Audio override '{current}' is not initialized; run audio-override-init first.");
        if (clipId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidDataException($"Clip identity '{clipId}' is not a file name.");
        var replacement = new AudioClipOverride(clipId, $"{ClipDirectory}/{clipId}.wav", loopStart);
        Publish(installation, staging =>
        {
            CopyDirectory(current, staging);
            AudioClipOverrideManifest manifest = ReadManifest(staging);
            Directory.CreateDirectory(Path.Combine(staging, ClipDirectory));
            File.Copy(wavPath, Path.Combine(staging, ClipDirectory, clipId + ".wav"), overwrite: true);
            WriteManifest(staging, manifest with
            {
                Clips = [.. manifest.Clips.Where(clip => clip.Id != clipId), replacement],
            });
        });
        return replacement;
    }

    /// <summary>
    /// Builds a candidate override in a staging directory, validates it with the stock clips, then
    /// swaps it in for the current override.
    /// </summary>
    private static void Publish(GameInstallation installation, Action<string> build)
    {
        string destination = Path.GetFullPath(installation.AudioOverrideDirectory);
        string parent = Path.GetDirectoryName(destination)
            ?? throw new InvalidDataException($"Audio override path '{destination}' has no parent directory.");
        Directory.CreateDirectory(parent);
        string staging = Path.Combine(parent, $".audio-override-{Guid.NewGuid():N}");
        string retired = Path.Combine(parent, $".audio-override-retired-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(staging);
            build(staging);
            // Validate the isolated candidate before it can become the selected override.
            _ = ClipAudioOverrides.Load(installation.AudioDirectory, staging);
            if (Directory.Exists(destination))
                Directory.Move(destination, retired);
            Directory.Move(staging, destination);
            // Only a completed swap retires the previous override; a failed one leaves it on disk.
            if (Directory.Exists(retired))
                Directory.Delete(retired, recursive: true);
        }
        finally
        {
            if (Directory.Exists(staging))
                Directory.Delete(staging, recursive: true);
        }
    }

    /// <summary>Reads an override's manifest.</summary>
    private static AudioClipOverrideManifest ReadManifest(string directory)
    {
        string path = Path.Combine(directory, AudioClipOverrideManifest.FileName);
        return JsonSerializer.Deserialize<AudioClipOverrideManifest>(File.ReadAllBytes(path), ClipAudioLibrary.JsonOptions)
            ?? throw new InvalidDataException($"Audio override manifest '{path}' deserialized to null.");
    }

    /// <summary>Writes an override's manifest.</summary>
    private static void WriteManifest(string directory, AudioClipOverrideManifest manifest) =>
        File.WriteAllText(
            Path.Combine(directory, AudioClipOverrideManifest.FileName),
            JsonSerializer.Serialize(manifest, ClipAudioLibrary.JsonOptions));

    /// <summary>Copies a directory tree.</summary>
    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
    }
}
