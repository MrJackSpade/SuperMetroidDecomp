using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// Describes how a replacement waveform obtains its loop point. Loop positions are PCM frame
/// indexes; preserving time rescales the stock position when an HD file changes sample rate.
/// </summary>
/// <param name="Kind">Policy used to derive the replacement waveform's intrinsic loop point.</param>
/// <param name="ExplicitSampleIndex">Zero-based decoded mono PCM frame, used only by the explicit-index policy.</param>
public readonly record struct PcmSampleLoopReplacement(
    PcmSampleLoopReplacementKind Kind,
    int ExplicitSampleIndex = 0)
{
    /// <summary>Preserves the existing loop's elapsed-time position by rate conversion rounded to the nearest PCM frame, or preserves the absence of a loop.</summary>
    public static PcmSampleLoopReplacement PreserveTime =>
        new(PcmSampleLoopReplacementKind.PreserveTime);

    /// <summary>Selects a replacement with no intrinsic loop point, regardless of the previous catalog entry.</summary>
    public static PcmSampleLoopReplacement Disabled =>
        new(PcmSampleLoopReplacementKind.Disabled);

    /// <summary>Selects an explicit zero-based PCM loop frame; the installer later validates that it lies within the replacement waveform.</summary>
    public static PcmSampleLoopReplacement At(int sampleIndex) =>
        new(PcmSampleLoopReplacementKind.ExplicitSampleIndex, sampleIndex);
}

/// <summary>Mutually exclusive policies for resolving a replacement sample's intrinsic PCM loop point.</summary>
public enum PcmSampleLoopReplacementKind
{
    /// <summary>Rescales the existing loop index to preserve its time at the replacement sample rate; an unlooped sample remains unlooped.</summary>
    PreserveTime,
    /// <summary>Removes the intrinsic loop point from the replacement metadata.</summary>
    Disabled,
    /// <summary>Uses the supplied decoded PCM frame index without sample-rate conversion.</summary>
    ExplicitSampleIndex,
}

/// <summary>
/// Installs a validated PCM16 mono WAV behind an existing stable sample ID and updates only
/// that catalog entry. Instrument assignments and every sequence reference remain unchanged.
/// </summary>
public static class PcmSampleReplacementInstaller
{
    /// <summary>Validates a mono PCM16 WAV and loop policy, then replaces one canonical sample's WAV and updates its rate, frame count, loop, and SHA-256 metadata.</summary>
    /// <param name="audioDirectory">Editable catalog directory containing the current-format audio manifest.</param>
    /// <param name="sampleId">Existing, unique, case-sensitive canonical sample identity; instrument and sequence references are retained.</param>
    /// <param name="replacementWavePath">Path to the replacement WAV, read and validated before any catalog file is overwritten.</param>
    /// <param name="loopReplacement">Policy resolving a loop index within the replacement's decoded PCM frames, or no loop.</param>
    /// <returns>The installed canonical metadata entry.</returns>
    /// <remarks>Staged files replace the WAV first and the manifest second; the pair is not an all-or-nothing transaction.</remarks>
    public static AudioCanonicalSampleMetadata Install(
        string audioDirectory,
        string sampleId,
        string replacementWavePath,
        PcmSampleLoopReplacement loopReplacement)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sampleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementWavePath);

        string root = Path.GetFullPath(audioDirectory);
        string manifestPath = Path.Combine(root, ExtractedAudioAssetCatalog.ManifestFileName);
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException($"Audio manifest was not found at '{manifestPath}'.", manifestPath);
        AudioAssetManifest manifest = JsonSerializer.Deserialize<AudioAssetManifest>(
            File.ReadAllText(manifestPath), AudioAssetJson.Options)
            ?? throw new InvalidDataException($"Audio manifest '{manifestPath}' deserialized to null.");
        if (manifest.FormatVersion != AudioAssetManifest.CurrentFormatVersion)
        {
            throw new InvalidDataException(
                $"Audio manifest format {manifest.FormatVersion} cannot install format " +
                $"{AudioAssetManifest.CurrentFormatVersion} replacements.");
        }

        int metadataIndex = -1;
        for (int index = 0; index < manifest.CanonicalSamples.Count; index++)
        {
            if (!manifest.CanonicalSamples[index].Id.Equals(sampleId, StringComparison.Ordinal))
                continue;
            if (metadataIndex >= 0)
                throw new InvalidDataException($"Audio manifest repeats canonical sample ID '{sampleId}'.");
            metadataIndex = index;
        }
        if (metadataIndex < 0)
            throw new InvalidDataException($"Audio manifest does not define canonical sample ID '{sampleId}'.");

        AudioCanonicalSampleMetadata previous = manifest.CanonicalSamples[metadataIndex];
        string sourcePath = Path.GetFullPath(replacementWavePath);
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"Replacement WAV was not found at '{sourcePath}'.", sourcePath);
        byte[] waveBytes = File.ReadAllBytes(sourcePath);
        (int sampleRate, short[] pcm) = PcmWaveFile.ReadMonoPcm16(waveBytes, sourcePath);
        int? loopSampleIndex = ResolveLoop(previous, sampleRate, loopReplacement);
        // Reuse the runtime value object's exhaustive rate/data/loop validation before any
        // generated asset is replaced on disk.
        _ = new ManagedPcmSample(sampleId, sampleRate, pcm, loopSampleIndex);

        string targetPath = ResolveContainedPath(root, previous.WavFile);
        var updated = previous with
        {
            SampleRate = sampleRate,
            SampleCount = pcm.Length,
            LoopSampleIndex = loopSampleIndex,
            Sha256 = Convert.ToHexString(SHA256.HashData(waveBytes)),
        };
        AudioCanonicalSampleMetadata[] canonical = [.. manifest.CanonicalSamples];
        canonical[metadataIndex] = updated;
        AudioAssetManifest updatedManifest = manifest with { CanonicalSamples = canonical };

        string waveTemporary = targetPath + $".{Guid.NewGuid():N}.tmp";
        string manifestTemporary = manifestPath + $".{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllBytes(waveTemporary, waveBytes);
            File.WriteAllText(
                manifestTemporary,
                JsonSerializer.Serialize(updatedManifest, AudioAssetJson.Options));
            File.Move(waveTemporary, targetPath, overwrite: true);
            File.Move(manifestTemporary, manifestPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(waveTemporary))
                File.Delete(waveTemporary);
            if (File.Exists(manifestTemporary))
                File.Delete(manifestTemporary);
        }
        return updated;
    }

    private static int? ResolveLoop(
        AudioCanonicalSampleMetadata previous,
        int replacementSampleRate,
        PcmSampleLoopReplacement replacement) => replacement.Kind switch
        {
            PcmSampleLoopReplacementKind.PreserveTime => previous.LoopSampleIndex is int oldLoop
                ? checked((int)(((long)oldLoop * replacementSampleRate + previous.SampleRate / 2) /
                    previous.SampleRate))
                : null,
            PcmSampleLoopReplacementKind.Disabled => null,
            PcmSampleLoopReplacementKind.ExplicitSampleIndex => replacement.ExplicitSampleIndex,
            _ => throw new InvalidDataException($"Unknown PCM loop replacement mode {replacement.Kind}."),
        };

    private static string ResolveContainedPath(string root, string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
            throw new InvalidDataException($"Audio manifest path '{relativePath}' must be relative.");
        string path = Path.GetFullPath(Path.Combine(root, relativePath));
        string prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Audio manifest path '{relativePath}' escapes '{root}'.");
        return path;
    }
}
