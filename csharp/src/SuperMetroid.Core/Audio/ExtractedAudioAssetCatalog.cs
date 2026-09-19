using System.Text.Json;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// Immutable address-keyed collection of extracted SPC upload streams. Runtime audio uses
/// the original cartridge address as its lookup key, preserving the 65816 queue protocol
/// without reading music or sample bytes from the ROM after startup.
/// </summary>
public sealed class ExtractedAudioAssetCatalog
{
    public const string ManifestFileName = "audio-manifest.json";
    private readonly IReadOnlyDictionary<int, byte[]> streams;
    private readonly IReadOnlyDictionary<int, ManagedPcmSampleBank> sampleBanks;
    private readonly IReadOnlyDictionary<int, IReadOnlyList<AudioInstrumentMetadata>>
        instrumentBanks;
    private readonly int canonicalSampleCount;

    private ExtractedAudioAssetCatalog(
        IReadOnlyDictionary<int, byte[]> streams,
        IReadOnlyDictionary<int, ManagedPcmSampleBank> sampleBanks,
        IReadOnlyDictionary<int, IReadOnlyList<AudioInstrumentMetadata>> instrumentBanks,
        int canonicalSampleCount)
    {
        this.streams = streams;
        this.sampleBanks = sampleBanks;
        this.instrumentBanks = instrumentBanks;
        this.canonicalSampleCount = canonicalSampleCount;
    }

    /// <summary>Number of physical WAV assets after logical deduplication.</summary>
    public int CanonicalSampleCount => canonicalSampleCount;

    /// <summary>Total source-number aliases exposed across common and music banks.</summary>
    public int SourceMappingCount => sampleBanks.Values.Sum(bank => bank.Samples.Count);

    /// <summary>Loads and validates every stream named by an extracted manifest.</summary>
    public static ExtractedAudioAssetCatalog Load(string audioDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioDirectory);
        string root = Path.GetFullPath(audioDirectory);
        string manifestPath = Path.Combine(root, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException(
                $"Extracted audio manifest was not found at '{manifestPath}'. Run the asset extractor's audio command.",
                manifestPath);
        }

        AudioAssetManifest manifest = JsonSerializer.Deserialize<AudioAssetManifest>(
            File.ReadAllText(manifestPath), AudioAssetJson.Options)
            ?? throw new InvalidDataException($"Audio manifest '{manifestPath}' deserialized to null.");
        if (manifest.FormatVersion != AudioAssetManifest.CurrentFormatVersion)
        {
            throw new InvalidDataException(
                $"Audio manifest '{manifestPath}' uses format {manifest.FormatVersion}; expected {AudioAssetManifest.CurrentFormatVersion}.");
        }

        Dictionary<int, byte[]> loaded = [];
        foreach (AudioUploadManifestEntry entry in manifest.Uploads)
        {
            string path = ResolveContainedPath(root, entry.StreamFile);
            byte[] bytes = File.ReadAllBytes(path);
            string actualHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
            if (!actualHash.Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Extracted audio stream '{path}' has SHA-256 {actualHash}, expected {entry.Sha256}.");
            }
            if (!loaded.TryAdd(entry.SnesAddress, bytes))
                throw new InvalidDataException($"Audio manifest repeats SNES upload address ${entry.SnesAddress:X6}.");
        }

        int[] missing = AudioAssetCatalogData.All
            .Select(definition => definition.SnesAddress)
            .Where(address => !loaded.ContainsKey(address))
            .ToArray();
        if (missing.Length != 0)
        {
            throw new InvalidDataException(
                $"Audio manifest omits required upload addresses: {string.Join(", ", missing.Select(address => $"${address:X6}"))}.");
        }
        Dictionary<string, ManagedPcmSample> canonicalSamples =
            LoadCanonicalSamples(root, manifest.CanonicalSamples);
        Dictionary<int, ManagedPcmSampleBank> banks = [];
        Dictionary<int, IReadOnlyList<AudioInstrumentMetadata>> instruments = [];
        foreach (AudioBankMetadata bank in manifest.Banks)
        {
            AudioUploadAssetDefinition definition = AudioAssetCatalogData.All.SingleOrDefault(
                candidate => candidate.SnesAddress == bank.SnesAddress)
                ?? throw new InvalidDataException(
                    $"Audio manifest contains unknown PCM bank address ${bank.SnesAddress:X6}.");
            if (bank.Name != definition.Name || bank.DataIndex != definition.DataIndex)
            {
                throw new InvalidDataException(
                    $"PCM bank ${bank.SnesAddress:X6} identity '{bank.Name}'/${bank.DataIndex:X2} " +
                    $"does not match '{definition.Name}'/${definition.DataIndex:X2}.");
            }
            Dictionary<byte, ManagedPcmSample> sources = [];
            foreach (AudioSampleMetadata mapping in bank.Samples)
            {
                if (!canonicalSamples.TryGetValue(mapping.SampleId, out ManagedPcmSample? sample))
                {
                    throw new InvalidDataException(
                        $"Audio bank '{bank.Name}' source ${mapping.Source:X2} names unknown sample '{mapping.SampleId}'.");
                }
                if (!sources.TryAdd(mapping.Source, sample))
                    throw new InvalidDataException($"Audio bank '{bank.Name}' repeats source ${mapping.Source:X2}.");
            }
            Dictionary<byte, byte> loopEntries = [];
            foreach (AudioSampleMetadata mapping in bank.Samples)
            {
                // A non-looping WAV still has a native DIR loop address. A live
                // SRCN change can enter that address without keying on this WAV.
                // Explicit WAV loops (including replacements) retain their own entry.
                if (sources[mapping.Source].LoopSampleIndex.HasValue)
                    continue;
                AudioSampleMetadata[] targets = bank.Samples
                    .Where(candidate => candidate.StartAddress == mapping.LoopAddress).ToArray();
                if (targets.Length == 0)
                    continue;
                if (targets.Any(target => target.SampleId != targets[0].SampleId))
                    throw new InvalidDataException($"PCM bank '{bank.Name}' has ambiguous loop entry ${mapping.LoopAddress:X4}.");
                loopEntries.Add(mapping.Source, targets[0].Source);
            }
            if (!banks.TryAdd(bank.SnesAddress, new ManagedPcmSampleBank(bank.Name, bank.SnesAddress, sources, loopEntries)))
                throw new InvalidDataException($"Audio manifest repeats sample bank address ${bank.SnesAddress:X6}.");
            if (!instruments.TryAdd(bank.SnesAddress, ValidateInstruments(bank)))
                throw new InvalidDataException($"Audio manifest repeats instrument bank address ${bank.SnesAddress:X6}.");
        }
        int[] missingBanks = AudioAssetCatalogData.All
            .Select(definition => definition.SnesAddress)
            .Where(address => !banks.ContainsKey(address))
            .ToArray();
        if (missingBanks.Length != 0)
        {
            throw new InvalidDataException(
                $"Audio manifest omits required PCM banks: {string.Join(", ", missingBanks.Select(address => $"${address:X6}"))}.");
        }
        HashSet<string> referencedIds = banks.Values
            .SelectMany(bank => bank.Samples.Values)
            .Select(sample => sample.Id)
            .ToHashSet(StringComparer.Ordinal);
        string[] unreferenced = canonicalSamples.Keys
            .Where(id => !referencedIds.Contains(id))
            .ToArray();
        if (unreferenced.Length != 0)
        {
            throw new InvalidDataException(
                $"Audio manifest contains unreferenced canonical samples: {string.Join(", ", unreferenced)}.");
        }
        return new ExtractedAudioAssetCatalog(loaded, banks, instruments, canonicalSamples.Count);
    }

    /// <summary>
    /// Validates immutable stock audio first, then selects a complete compatible catalog from
    /// the persistent override directory when one exists. An invalid override never silently
    /// falls back to stock, and a valid override never conceals broken stock installation data.
    /// </summary>
    public static ExtractedAudioAssetCatalog Load(string stockDirectory, string overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(overrideDirectory);
        string stockRoot = Path.GetFullPath(stockDirectory);
        string overrideRoot = Path.GetFullPath(overrideDirectory);
        ExtractedAudioAssetCatalog stock = Load(stockRoot);
        string overrideManifestPath = Path.Combine(overrideRoot, ManifestFileName);
        if (!File.Exists(overrideManifestPath))
            return stock;

        AudioAssetManifest stockManifest = ReadManifest(stockRoot);
        AudioAssetManifest overrideManifest = ReadManifest(overrideRoot);
        ValidateOverrideCompatibility(stockManifest, overrideManifest, overrideManifestPath);
        return Load(overrideRoot);
    }

    /// <summary>Returns the exact terminated upload stream for a cartridge command.</summary>
    public ReadOnlyMemory<byte> GetUpload(int snesAddress) =>
        streams.TryGetValue(snesAddress, out byte[]? bytes)
            ? bytes
            : throw new InvalidDataException(
                $"Audio command references unmapped extracted upload address ${snesAddress:X6}.");

    /// <summary>Returns the replaceable PCM source map installed by the same upload.</summary>
    public ManagedPcmSampleBank GetSampleBank(int snesAddress) =>
        sampleBanks.TryGetValue(snesAddress, out ManagedPcmSampleBank? bank)
            ? bank
            : throw new InvalidDataException(
                $"Audio command references unmapped PCM bank address ${snesAddress:X6}.");

    /// <summary>
    /// Returns the editable instrument table installed after the corresponding opaque
    /// upload. The ordered records retain the SPC driver's native six-byte identities.
    /// </summary>
    public IReadOnlyList<AudioInstrumentMetadata> GetInstrumentBank(int snesAddress) =>
        instrumentBanks.TryGetValue(snesAddress, out IReadOnlyList<AudioInstrumentMetadata>? bank)
            ? bank
            : throw new InvalidDataException(
                $"Audio command references unmapped instrument bank address ${snesAddress:X6}.");

    private static System.Collections.ObjectModel.ReadOnlyCollection<AudioInstrumentMetadata>
        ValidateInstruments(
        AudioBankMetadata bank)
    {
        if (bank.Instruments.Count != SpcDriverData.Ram.InstrumentCount)
        {
            throw new InvalidDataException(
                $"Audio bank '{bank.Name}' defines {bank.Instruments.Count} instruments; " +
                $"expected {SpcDriverData.Ram.InstrumentCount}.");
        }

        Dictionary<int, AudioInstrumentMetadata> byId = [];
        foreach (AudioInstrumentMetadata instrument in bank.Instruments)
        {
            if ((uint)instrument.Instrument >= SpcDriverData.Ram.InstrumentCount)
            {
                throw new InvalidDataException(
                    $"Audio bank '{bank.Name}' instrument {instrument.Instrument} is outside " +
                    $"0..{SpcDriverData.Ram.InstrumentCount - 1}.");
            }
            if (instrument.UsesNoise !=
                ((instrument.SourceOrNoiseRate & SpcDriverData.Instruments.NoiseMarker) != 0))
            {
                throw new InvalidDataException(
                    $"Audio bank '{bank.Name}' instrument {instrument.Instrument} has inconsistent " +
                    "usesNoise and sourceOrNoiseRate fields.");
            }
            if (!byId.TryAdd(instrument.Instrument, instrument))
            {
                throw new InvalidDataException(
                    $"Audio bank '{bank.Name}' repeats instrument {instrument.Instrument}.");
            }
        }

        var ordered = new AudioInstrumentMetadata[SpcDriverData.Ram.InstrumentCount];
        for (int instrument = 0; instrument < ordered.Length; instrument++)
        {
            if (!byId.TryGetValue(instrument, out AudioInstrumentMetadata? definition))
                throw new InvalidDataException($"Audio bank '{bank.Name}' omits instrument {instrument}.");
            ordered[instrument] = definition;
        }
        return Array.AsReadOnly(ordered);
    }

    private static Dictionary<string, ManagedPcmSample> LoadCanonicalSamples(
        string root,
        IReadOnlyList<AudioCanonicalSampleMetadata> definitions)
    {
        Dictionary<string, ManagedPcmSample> samples = new(StringComparer.Ordinal);
        HashSet<string> logicalContent = new(StringComparer.Ordinal);
        foreach (AudioCanonicalSampleMetadata definition in definitions)
        {
            string path = ResolveContainedPath(root, definition.WavFile);
            byte[] bytes = File.ReadAllBytes(path);
            string actualHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
            if (!actualHash.Equals(definition.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"PCM sample '{path}' has SHA-256 {actualHash}, expected {definition.Sha256}. " +
                    "Update the manifest hash explicitly when installing a replacement sample.");
            }
            (int sampleRate, short[] pcm) = PcmWaveFile.ReadMonoPcm16(bytes, path);
            if (sampleRate != definition.SampleRate || pcm.Length != definition.SampleCount)
            {
                throw new InvalidDataException(
                    $"PCM sample '{path}' is {sampleRate} Hz/{pcm.Length} samples; manifest declares " +
                    $"{definition.SampleRate} Hz/{definition.SampleCount} samples.");
            }
            var sample = new ManagedPcmSample(
                definition.Id, sampleRate, pcm, definition.LoopSampleIndex);
            if (!samples.TryAdd(definition.Id, sample))
                throw new InvalidDataException($"Audio manifest repeats canonical sample ID '{definition.Id}'.");
            byte[] pcmBytes = new byte[checked(pcm.Length * sizeof(short))];
            Buffer.BlockCopy(pcm, 0, pcmBytes, 0, pcmBytes.Length);
            string signature = $"{sampleRate}:{definition.LoopSampleIndex?.ToString() ?? "-"}:" +
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pcmBytes));
            if (!logicalContent.Add(signature))
            {
                throw new InvalidDataException(
                    $"Canonical sample '{definition.Id}' duplicates another WAV/loop pair instead of aliasing it.");
            }
        }
        return samples;
    }

    private static string ResolveContainedPath(string root, string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
            throw new InvalidDataException($"Audio manifest path '{relativePath}' must be relative.");
        string path = Path.GetFullPath(Path.Combine(root, relativePath));
        string prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Audio manifest path '{relativePath}' escapes '{root}'.");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Extracted audio stream '{path}' is missing.", path);
        return path;
    }

    private static AudioAssetManifest ReadManifest(string root)
    {
        string path = Path.Combine(root, ManifestFileName);
        return JsonSerializer.Deserialize<AudioAssetManifest>(
            File.ReadAllText(path), AudioAssetJson.Options)
            ?? throw new InvalidDataException($"Audio manifest '{path}' deserialized to null.");
    }

    private static void ValidateOverrideCompatibility(
        AudioAssetManifest stock,
        AudioAssetManifest selected,
        string selectedPath)
    {
        if (selected.FormatVersion != stock.FormatVersion)
        {
            throw new InvalidDataException(
                $"Audio override '{selectedPath}' uses format {selected.FormatVersion}; " +
                $"installed stock uses format {stock.FormatVersion}.");
        }
        if (selected.Uploads.Count != stock.Uploads.Count ||
            !selected.Uploads.Zip(stock.Uploads).All(pair => pair.First == pair.Second))
        {
            throw new InvalidDataException(
                $"Audio override '{selectedPath}' changes opaque SPC uploads or their identity. " +
                "Only decoded samples, instruments, and authored sequence definitions are replaceable.");
        }
        if (selected.Banks.Count != stock.Banks.Count)
            throw new InvalidDataException($"Audio override '{selectedPath}' changes the bank catalog.");
        for (int index = 0; index < stock.Banks.Count; index++)
        {
            AudioBankMetadata expected = stock.Banks[index];
            AudioBankMetadata actual = selected.Banks[index];
            if (actual.Name != expected.Name || actual.DataIndex != expected.DataIndex ||
                actual.SnesAddress != expected.SnesAddress ||
                !actual.TrackPointers.SequenceEqual(expected.TrackPointers) ||
                actual.Samples.Count != expected.Samples.Count ||
                !actual.Samples.Zip(expected.Samples).All(pair => pair.First == pair.Second))
            {
                throw new InvalidDataException(
                    $"Audio override '{selectedPath}' changes routing or sample-source identity " +
                    $"for stock bank '{expected.Name}'.");
            }
        }
        if (selected.CanonicalSamples.Count != stock.CanonicalSamples.Count ||
            !selected.CanonicalSamples.Select(sample => sample.Id).SequenceEqual(
                stock.CanonicalSamples.Select(sample => sample.Id), StringComparer.Ordinal))
        {
            throw new InvalidDataException(
                $"Audio override '{selectedPath}' changes stable canonical sample IDs.");
        }
        if (selected.SoundLibraries.Count != stock.SoundLibraries.Count ||
            !selected.SoundLibraries.Zip(stock.SoundLibraries).All(pair =>
                pair.First.Library == pair.Second.Library &&
                pair.First.Effects.Count == pair.Second.Effects.Count &&
                pair.First.Effects.Zip(pair.Second.Effects).All(effect => effect.First == effect.Second)))
        {
            throw new InvalidDataException(
                $"Audio override '{selectedPath}' changes compiled SFX routing metadata.");
        }
    }
}

/// <summary>Serializable root of the inspectable extracted-audio catalog.</summary>
public sealed record AudioAssetManifest(
    int FormatVersion,
    IReadOnlyList<AudioUploadManifestEntry> Uploads,
    IReadOnlyList<AudioCanonicalSampleMetadata> CanonicalSamples,
    IReadOnlyList<AudioBankMetadata> Banks,
    IReadOnlyList<AudioSoundLibraryMetadata> SoundLibraries)
{
    public const int CurrentFormatVersion = 2;
}

/// <summary>Manifest identity and integrity information for one opaque upload stream.</summary>
public sealed record AudioUploadManifestEntry(
    string Name,
    int SnesAddress,
    byte DataIndex,
    string StreamFile,
    int ByteLength,
    string Sha256);

/// <summary>Decoded, human-readable metadata derived from one common-plus-music APU image.</summary>
public sealed record AudioBankMetadata(
    string Name,
    byte DataIndex,
    int SnesAddress,
    IReadOnlyList<ushort> TrackPointers,
    IReadOnlyList<AudioInstrumentMetadata> Instruments,
    IReadOnlyList<AudioSampleMetadata> Samples);

/// <summary>The six bytes consumed by the SPC driver's set-instrument command.</summary>
public sealed record AudioInstrumentMetadata(
    int Instrument,
    bool UsesNoise,
    byte SourceOrNoiseRate,
    byte Adsr1,
    byte Adsr2,
    byte Gain,
    ushort PitchBase);

/// <summary>One validated BRR directory entry and its canonical PCM sample alias.</summary>
public sealed record AudioSampleMetadata(
    byte Source,
    ushort StartAddress,
    ushort LoopAddress,
    int BlockCount,
    string SampleId);

/// <summary>One canonical, replaceable PCM asset shared by every equivalent bank source.</summary>
public sealed record AudioCanonicalSampleMetadata(
    string Id,
    string WavFile,
    int SampleRate,
    int SampleCount,
    int? LoopSampleIndex,
    string Sha256);

/// <summary>Static SFX routing metadata used by the managed SPC driver.</summary>
public sealed record AudioSoundLibraryMetadata(
    int Library,
    IReadOnlyList<AudioSoundEffectMetadata> Effects);

/// <summary>Pointer and voice-allocation class for a one-based SFX command.</summary>
public sealed record AudioSoundEffectMetadata(byte Command, ushort StreamPointer, byte Configuration);

internal static class AudioAssetJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
}
