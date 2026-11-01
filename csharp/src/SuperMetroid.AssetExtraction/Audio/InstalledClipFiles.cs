using System.Security.Cryptography;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The installed clip files (#1305): reads a clip's WAV beneath the audio root, or an override's replacement from its
/// own path, verifying the file's SHA-256 against the manifest and its rate against the clips' rate.
/// </summary>
/// <param name="root">The audio root the manifest's paths are relative to.</param>
/// <param name="sampleRate">The manifest's clip rate.</param>
/// <param name="overrideFiles">Each override replacement clip's absolute path; every other clip lives beneath <paramref name="root"/>.</param>
internal sealed class InstalledClipFiles(string root, int sampleRate, IReadOnlyDictionary<AudioClipFile, string> overrideFiles)
    : IAudioClipSource
{
    /// <summary>Bytes of the minimal RIFF/WAVE header the exporter writes before the samples.</summary>
    private const int WaveHeaderBytes = 44;

    /// <summary>Bytes per 16-bit stereo frame.</summary>
    private const int BytesPerFrame = PcmSampleFormat.StereoChannelCount * sizeof(short);

    /// <inheritdoc />
    /// <exception cref="InvalidDataException">The file's hash, encoding or rate does not match the manifest.</exception>
    public short[] ReadSamples(AudioClipFile clip)
    {
        Ensure.NotNull(clip);
        string path = overrideFiles.TryGetValue(clip, out string? replacement) ? replacement : ResolveContainedPath(root, clip.Path);
        byte[] bytes = File.ReadAllBytes(path);
        string hash = Convert.ToHexString(SHA256.HashData(bytes));
        if (!hash.Equals(clip.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Audio clip '{path}' has SHA-256 {hash}, expected {clip.Sha256}.");
        (int rate, short[] samples) = PcmWaveFile.ReadStereoPcm16(bytes, path);
        if (rate != sampleRate)
            throw new InvalidDataException($"Audio clip '{path}' is {rate} Hz, expected {sampleRate}.");
        return samples;
    }

    /// <summary>Checks a clip file exists beneath the root with the size its frame count implies.</summary>
    /// <exception cref="FileNotFoundException">The file is missing.</exception>
    /// <exception cref="InvalidDataException">The path escapes the root or the file's size is wrong.</exception>
    internal static void ValidateFileSize(string root, AudioClipFile clip)
    {
        string path = ResolveContainedPath(root, clip.Path);
        long expected = WaveHeaderBytes + clip.Frames * BytesPerFrame;
        long actual = new FileInfo(path).Length;
        if (actual != expected)
            throw new InvalidDataException($"Audio clip '{path}' is {actual} bytes, expected {expected}.");
    }

    /// <summary>Resolves a manifest path beneath the root, rejecting escapes and missing files.</summary>
    private static string ResolveContainedPath(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new InvalidDataException($"Audio clip path '{relativePath}' must be relative.");
        string path = Path.GetFullPath(Path.Combine(root, relativePath));
        string prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Audio clip path '{relativePath}' escapes '{root}'.");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Audio clip '{path}' is missing.", path);
        return path;
    }
}
