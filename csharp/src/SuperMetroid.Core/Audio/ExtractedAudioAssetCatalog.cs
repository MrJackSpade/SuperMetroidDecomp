using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// Immutable address-keyed collection of extracted SPC upload streams. Runtime audio uses
/// the original cartridge address as its lookup key, preserving the 65816 queue protocol
/// without reading music or sample bytes from the ROM after startup.
/// </summary>
public sealed class ExtractedAudioAssetCatalog
{
    /// <summary>JSON manifest filename beneath the selected audio root, identifying upload streams, canonical WAV samples, and authored bank/SFX definitions.</summary>
    public const string ManifestFileName = "audio-manifest.json";
    private readonly IReadOnlyDictionary<int, byte[]> streams;
    private readonly IReadOnlyDictionary<int, ManagedPcmSampleBank> sampleBanks;
    private readonly IReadOnlyDictionary<int, IReadOnlyList<AudioInstrumentMetadata>>
        instrumentBanks;
    private readonly IReadOnlyDictionary<int, AudioBankMetadata> musicBanks;
    private readonly int canonicalSampleCount;

    private ExtractedAudioAssetCatalog(
        IReadOnlyDictionary<int, byte[]> streams,
        IReadOnlyDictionary<int, ManagedPcmSampleBank> sampleBanks,
        IReadOnlyDictionary<int, IReadOnlyList<AudioInstrumentMetadata>> instrumentBanks,
        IReadOnlyDictionary<int, AudioBankMetadata> musicBanks,
        IReadOnlyList<AudioSoundProgramMetadata> soundPrograms,
        IReadOnlyList<AudioSoundLibraryMetadata> soundLibraries,
        int canonicalSampleCount,
        string contentIdentity)
    {
        this.streams = streams;
        this.sampleBanks = sampleBanks;
        this.instrumentBanks = instrumentBanks;
        this.musicBanks = musicBanks;
        SoundPrograms = soundPrograms;
        SoundLibraries = soundLibraries;
        this.canonicalSampleCount = canonicalSampleCount;
        ContentIdentity = contentIdentity;
    }

    /// <summary>
    /// Stable SHA-256 identity of the validated selected manifest. The canonical JSON contains
    /// every upload/WAV hash plus all editable routing, instrument, music, and SFX definitions,
    /// so formatting-only changes do not alter this value while any audible edit does.
    /// </summary>
    public string ContentIdentity { get; }

    /// <summary>Loads and validates every stream named by an extracted manifest.</summary>
    /// <param name="audioDirectory">Audio root containing the manifest and all of its relative upload/WAV files; referenced paths must remain beneath this root.</param>
    /// <returns>An address-keyed catalog whose upload and PCM bytes are loaded into memory and whose authored definitions have passed compatibility validation.</returns>
    /// <exception cref="InvalidDataException">The schema, hashes, required bank identities, sample aliases, or authored definitions are invalid.</exception>
    /// <exception cref="FileNotFoundException">The manifest or a referenced audio file is missing.</exception>
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

        AudioAssetManifest manifest = DeserializeManifest(manifestPath);
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
        Dictionary<int, AudioBankMetadata> musicBanks = [];
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
            ValidateMusicBank(bank);
            if (!musicBanks.TryAdd(bank.SnesAddress, bank))
                throw new InvalidDataException($"Audio manifest repeats music bank address ${bank.SnesAddress:X6}.");
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
        (IReadOnlyList<AudioSoundProgramMetadata> soundPrograms,
            IReadOnlyList<AudioSoundLibraryMetadata> soundLibraries) =
            ValidateSoundDefinitions(manifest);
        return new ExtractedAudioAssetCatalog(
            loaded,
            banks,
            instruments,
            musicBanks,
            soundPrograms,
            soundLibraries,
            canonicalSamples.Count,
            ComputeContentIdentity(manifest));
    }

    /// <summary>SHA-256 of the canonical serialized manifest, hashed as it streams out.</summary>
    private static string ComputeContentIdentity(AudioAssetManifest manifest)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using (var hashing = new HashingWriteStream(hash))
            JsonSerializer.Serialize(hashing, manifest, AudioAssetJson.Options);
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>A write-only stream that appends every byte to an incremental hash.</summary>
    private sealed class HashingWriteStream(IncrementalHash hash) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) => hash.AppendData(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => hash.AppendData(buffer);
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    /// <summary>
    /// Validates immutable stock audio first, then selects a complete compatible catalog from
    /// the persistent override directory when one exists. An invalid override never silently
    /// falls back to stock, and a valid override never conceals broken stock installation data.
    /// </summary>
    /// <param name="stockDirectory">Complete stock catalog, validated before override selection.</param>
    /// <param name="overrideDirectory">Persistent replacement catalog root; absence of its manifest selects stock without merging individual files.</param>
    /// <returns>The complete validated override catalog when present and compatible, otherwise the validated stock catalog.</returns>
    /// <exception cref="InvalidDataException">Stock or selected content is invalid, or overrides change fixed upload, bank, sample-source, or program/routing identities.</exception>
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

    /// <summary>Returns the decoded authored music definitions installed by one upload.</summary>
    public AudioBankMetadata GetMusicBank(int snesAddress) =>
        musicBanks.TryGetValue(snesAddress, out AudioBankMetadata? bank)
            ? bank
            : throw new InvalidDataException(
                $"Audio command references unmapped music bank address ${snesAddress:X6}.");

    /// <summary>Decoded, address-stable authored SFX channel programs.</summary>
    public IReadOnlyList<AudioSoundProgramMetadata> SoundPrograms { get; }

    /// <summary>Stable SFX command IDs and their compiled routing/allocation metadata.</summary>
    public IReadOnlyList<AudioSoundLibraryMetadata> SoundLibraries { get; }

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

    private static void ValidateMusicBank(AudioBankMetadata bank)
    {
        if (bank.TrackPointers is null || bank.MusicTracks is null ||
            bank.MusicPhrases is null || bank.MusicPrograms is null)
            throw new InvalidDataException($"Audio bank '{bank.Name}' has an incomplete music catalog.");
        if (bank.TrackPointers.Count != bank.MusicTracks.Count)
        {
            throw new InvalidDataException(
                $"Audio bank '{bank.Name}' has {bank.TrackPointers.Count} track pointers but " +
                $"{bank.MusicTracks.Count} decoded tracks.");
        }
        Dictionary<ushort, AudioMusicPhraseMetadata> phrases = [];
        foreach (AudioMusicPhraseMetadata phrase in bank.MusicPhrases)
        {
            string expectedId = $"music-{bank.DataIndex:x2}-phrase-{phrase.Address:x4}";
            if (phrase.Id != expectedId || !phrases.TryAdd(phrase.Address, phrase))
                throw new InvalidDataException($"Audio bank '{bank.Name}' has invalid phrase identity '{phrase.Id}'.");
        }
        Dictionary<ushort, AudioMusicProgramMetadata> programs = [];
        foreach (AudioMusicProgramMetadata program in bank.MusicPrograms)
        {
            string expectedId = $"music-{bank.DataIndex:x2}-program-{program.Address:x4}";
            if (program.Id != expectedId || !programs.TryAdd(program.Address, program))
                throw new InvalidDataException($"Audio bank '{bank.Name}' has invalid program identity '{program.Id}'.");
        }
        for (int trackIndex = 0; trackIndex < bank.MusicTracks.Count; trackIndex++)
        {
            AudioMusicTrackMetadata track = bank.MusicTracks[trackIndex];
            string expectedId = $"music-{bank.DataIndex:x2}-track-{trackIndex:00}";
            if (track.Track != trackIndex || track.Id != expectedId ||
                track.Address != bank.TrackPointers[trackIndex])
                throw new InvalidDataException($"Audio bank '{bank.Name}' has invalid track identity '{track.Id}'.");
            _ = SpcMusicDefinitionCodec.EncodeTrack(track);
            foreach (AudioMusicTrackInstructionMetadata instruction in track.Instructions)
            {
                if (instruction.Operation == AudioMusicInstructionOperations.PlayPhrase &&
                    !phrases.ContainsKey(instruction.Value))
                {
                    throw new InvalidDataException(
                        $"Music track '{track.Id}' references missing phrase ${instruction.Value:X4}.");
                }
            }
        }
        foreach (AudioMusicProgramMetadata program in bank.MusicPrograms)
        {
            _ = SpcMusicDefinitionCodec.EncodeProgram(program);
            foreach (AudioMusicInstructionMetadata instruction in program.Instructions)
            {
                if (instruction.Opcode != (byte)SpcMusicEffect.CallPattern)
                    continue;
                ushort target = unchecked((ushort)(
                    instruction.Arguments[0] | (instruction.Arguments[1] << 8)));
                if (!programs.ContainsKey(target))
                {
                    throw new InvalidDataException(
                        $"Music program '{program.Id}' calls missing program ${target:X4}.");
                }
            }
        }
        _ = SpcMusicDefinitionCodec.CompileBank(bank);
    }

    private static (
        IReadOnlyList<AudioSoundProgramMetadata> Programs,
        IReadOnlyList<AudioSoundLibraryMetadata> Libraries) ValidateSoundDefinitions(
        AudioAssetManifest manifest)
    {
        if (manifest.SoundPrograms is null)
            throw new InvalidDataException("Audio manifest has no decoded sound-program catalog.");
        if (manifest.SoundLibraries is null)
            throw new InvalidDataException("Audio manifest has no sound-library catalog.");
        Dictionary<string, AudioSoundProgramMetadata> programsById = new(StringComparer.Ordinal);
        HashSet<ushort> addresses = [];
        foreach (AudioSoundProgramMetadata program in manifest.SoundPrograms)
        {
            if (string.IsNullOrWhiteSpace(program.Id))
                throw new InvalidDataException("Audio manifest contains a sound program with no stable ID.");
            if (!programsById.TryAdd(program.Id, program))
                throw new InvalidDataException($"Audio manifest repeats sound program ID '{program.Id}'.");
            if (!addresses.Add(program.Address))
                throw new InvalidDataException($"Audio manifest repeats sound program address ${program.Address:X4}.");
            if (program.ByteCapacity <= 0 || program.Address + program.ByteCapacity > SpcDriverData.ApuRamSize)
            {
                throw new InvalidDataException(
                    $"Audio sound program '{program.Id}' has invalid fixed slot " +
                    $"${program.Address:X4}+{program.ByteCapacity}.");
            }
            _ = SpcSoundEffectProgramCodec.Encode(program);
        }

        if (manifest.SoundLibraries.Count != SpcDriverData.SoundEffects.LibraryCount)
        {
            throw new InvalidDataException(
                $"Audio manifest defines {manifest.SoundLibraries.Count} SFX libraries; " +
                $"expected {SpcDriverData.SoundEffects.LibraryCount}.");
        }
        for (int libraryIndex = 0; libraryIndex < manifest.SoundLibraries.Count; libraryIndex++)
        {
            AudioSoundLibraryMetadata library = manifest.SoundLibraries[libraryIndex];
            if (library.Effects is null)
                throw new InvalidDataException($"Audio SFX library {library.Library} has no effect list.");
            if (library.Library != libraryIndex + 1)
                throw new InvalidDataException($"Audio SFX library index {libraryIndex} is identified as {library.Library}.");
            int commandCount = SpcSoundEffectTables.CommandCount(libraryIndex);
            if (library.Effects.Count != commandCount)
            {
                throw new InvalidDataException(
                    $"Audio SFX library {library.Library} defines {library.Effects.Count} effects; " +
                    $"expected {commandCount}.");
            }
            for (int effectIndex = 0; effectIndex < library.Effects.Count; effectIndex++)
            {
                AudioSoundEffectMetadata effect = library.Effects[effectIndex];
                if (effect.ChannelPrograms is null)
                    throw new InvalidDataException($"Audio effect '{effect.Id}' has no channel-program list.");
                byte command = unchecked((byte)(effectIndex + 1));
                string id = $"sfx-{library.Library}-{command:x2}";
                if (effect.Command != command || effect.Id != id ||
                    effect.StreamPointer != SpcSoundEffectTables.StreamPointer(libraryIndex, command) ||
                    effect.Configuration != SpcSoundEffectTables.Configuration(libraryIndex, command))
                {
                    throw new InvalidDataException(
                        $"Audio effect {library.Library}:${command:X2} changes compiled identity, " +
                        "routing pointer, or voice-allocation configuration.");
                }
                int channelCount = SpcSoundEffectTables.GetVoiceCount(
                    libraryIndex,
                    effect.Configuration);
                if (effect.ChannelPrograms.Count != channelCount)
                {
                    throw new InvalidDataException(
                        $"Audio effect '{effect.Id}' names {effect.ChannelPrograms.Count} channel programs; " +
                        $"its compiled allocation requires {channelCount}.");
                }
                foreach (string programId in effect.ChannelPrograms)
                {
                    if (!programsById.ContainsKey(programId))
                    {
                        throw new InvalidDataException(
                            $"Audio effect '{effect.Id}' references unknown sound program '{programId}'.");
                    }
                }
            }
        }
        return (Array.AsReadOnly(manifest.SoundPrograms.ToArray()),
            Array.AsReadOnly(manifest.SoundLibraries.ToArray()));
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
        return DeserializeManifest(Path.Combine(root, ManifestFileName));
    }

    /// <summary>
    /// Streams the multi-megabyte manifest from its UTF-8 file rather than decoding it into one
    /// UTF-16 string first; the stream reader accepts the same optional byte-order mark.
    /// </summary>
    private static AudioAssetManifest DeserializeManifest(string path)
    {
        using var file = File.OpenRead(path);
        return JsonSerializer.Deserialize<AudioAssetManifest>(file, AudioAssetJson.Options)
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
                actual.MusicTracks.Count != expected.MusicTracks.Count ||
                !actual.MusicTracks.Zip(expected.MusicTracks).All(pair =>
                    pair.First.Track == pair.Second.Track &&
                    pair.First.Id == pair.Second.Id &&
                    pair.First.Address == pair.Second.Address &&
                    pair.First.ByteCapacity == pair.Second.ByteCapacity) ||
                actual.MusicPhrases.Count != expected.MusicPhrases.Count ||
                !actual.MusicPhrases.Zip(expected.MusicPhrases).All(pair =>
                    pair.First.Id == pair.Second.Id &&
                    pair.First.Address == pair.Second.Address &&
                    pair.First.ChannelPrograms.SequenceEqual(
                        pair.Second.ChannelPrograms, StringComparer.Ordinal)) ||
                actual.MusicPrograms.Count != expected.MusicPrograms.Count ||
                !actual.MusicPrograms.Zip(expected.MusicPrograms).All(pair =>
                    pair.First.Id == pair.Second.Id &&
                    pair.First.Address == pair.Second.Address &&
                    pair.First.ByteCapacity == pair.Second.ByteCapacity) ||
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
        if (selected.SoundPrograms.Count != stock.SoundPrograms.Count ||
            !selected.SoundPrograms.Zip(stock.SoundPrograms).All(pair =>
                pair.First.Id == pair.Second.Id &&
                pair.First.Address == pair.Second.Address &&
                pair.First.ByteCapacity == pair.Second.ByteCapacity) ||
            selected.SoundLibraries.Count != stock.SoundLibraries.Count ||
            !selected.SoundLibraries.Zip(stock.SoundLibraries).All(pair =>
                pair.First.Library == pair.Second.Library &&
                pair.First.Effects.Count == pair.Second.Effects.Count &&
                pair.First.Effects.Zip(pair.Second.Effects).All(effect =>
                    effect.First.Command == effect.Second.Command &&
                    effect.First.Id == effect.Second.Id &&
                    effect.First.StreamPointer == effect.Second.StreamPointer &&
                    effect.First.Configuration == effect.Second.Configuration &&
                    effect.First.ChannelPrograms.SequenceEqual(
                        effect.Second.ChannelPrograms, StringComparer.Ordinal))))
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
    IReadOnlyList<AudioSoundProgramMetadata> SoundPrograms,
    IReadOnlyList<AudioSoundLibraryMetadata> SoundLibraries)
{
    /// <summary>Accepted manifest schema version 4, including canonical PCM aliases and decoded, address-stable music/SFX programs.</summary>
    public const int CurrentFormatVersion = 4;
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
    IReadOnlyList<AudioMusicTrackMetadata> MusicTracks,
    IReadOnlyList<AudioMusicPhraseMetadata> MusicPhrases,
    IReadOnlyList<AudioMusicProgramMetadata> MusicPrograms,
    IReadOnlyList<AudioInstrumentMetadata> Instruments,
    IReadOnlyList<AudioSampleMetadata> Samples);

/// <summary>One numbered music track's top-level phrase-flow program.</summary>
public sealed record AudioMusicTrackMetadata(
    int Track,
    string Id,
    ushort Address,
    int ByteCapacity,
    IReadOnlyList<AudioMusicTrackInstructionMetadata> Instructions);

/// <summary>One phrase-flow operation; repeat uses Target while other operations do not.</summary>
public sealed record AudioMusicTrackInstructionMetadata(
    string Operation,
    ushort Value,
    ushort? Target);

/// <summary>Eight-channel routing table selected by a top-level track instruction.</summary>
public sealed record AudioMusicPhraseMetadata(
    string Id,
    ushort Address,
    IReadOnlyList<string?> ChannelPrograms);

/// <summary>One bounded authored channel/subroutine program.</summary>
public sealed record AudioMusicProgramMetadata(
    string Id,
    ushort Address,
    int ByteCapacity,
    IReadOnlyList<AudioMusicInstructionMetadata> Instructions);

/// <summary>
/// One channel command. Timing contains the optional length/articulation prefix; Arguments
/// contains effect operands. Opcode is retained for exact lossless recompilation.
/// </summary>
public sealed record AudioMusicInstructionMetadata(
    string Operation,
    byte Opcode,
    IReadOnlyList<byte> Timing,
    IReadOnlyList<byte> Arguments);

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

/// <summary>A bounded, address-stable resident SPC sound-effect channel program.</summary>
public sealed record AudioSoundProgramMetadata(
    string Id,
    ushort Address,
    int ByteCapacity,
    IReadOnlyList<AudioSoundInstructionMetadata> Instructions);

/// <summary>One named operation and its documented byte operands.</summary>
public sealed record AudioSoundInstructionMetadata(
    string Operation,
    IReadOnlyList<byte> Arguments);

/// <summary>Stable identity, routing and voice-allocation class for a one-based SFX command.</summary>
public sealed record AudioSoundEffectMetadata(
    byte Command,
    string Id,
    ushort StreamPointer,
    byte Configuration,
    IReadOnlyList<string> ChannelPrograms);

internal static class AudioAssetJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new ExactByteListConverter(), new MusicInstructionConverter() },
    };

    /// <summary>
    /// Reads each music instruction straight into its record. The generic constructor path
    /// allocated argument state per instruction across thousands of instructions per load.
    /// Semantics match the default record binding: unknown members are skipped, the last
    /// duplicate wins, absent members take their defaults, and the written JSON is identical.
    /// </summary>
    private sealed class MusicInstructionConverter : JsonConverter<AudioMusicInstructionMetadata>
    {
        // Operation names repeat across every program; keep one instance of each.
        [ThreadStatic] private static Dictionary<string, string>? operations;

        public override AudioMusicInstructionMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected a music instruction object.");
            string? operation = null;
            byte opcode = 0;
            IReadOnlyList<byte>? timing = null, arguments = null;
            var bytes = (JsonConverter<IReadOnlyList<byte>>)options.GetConverter(typeof(IReadOnlyList<byte>));
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected a music instruction property.");
                if (reader.ValueTextEquals("operation"u8))
                {
                    reader.Read();
                    operation = reader.TokenType == JsonTokenType.Null ? null : Operation(ref reader);
                }
                else if (reader.ValueTextEquals("opcode"u8))
                {
                    reader.Read();
                    opcode = reader.GetByte();
                }
                else if (reader.ValueTextEquals("timing"u8))
                {
                    reader.Read();
                    timing = reader.TokenType == JsonTokenType.Null ? null : bytes.Read(ref reader, typeof(IReadOnlyList<byte>), options);
                }
                else if (reader.ValueTextEquals("arguments"u8))
                {
                    reader.Read();
                    arguments = reader.TokenType == JsonTokenType.Null ? null : bytes.Read(ref reader, typeof(IReadOnlyList<byte>), options);
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }
            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException("Music instruction is not terminated.");
            return new AudioMusicInstructionMetadata(operation!, opcode, timing!, arguments!);
        }

        private static string Operation(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Music instruction operation must be a string.");
            int length = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
            Span<char> text = length <= 128 ? stackalloc char[length] : new char[length];
            int written = reader.CopyString(text);
            var lookup = (operations ??= new(StringComparer.Ordinal)).GetAlternateLookup<ReadOnlySpan<char>>();
            if (lookup.TryGetValue(text[..written], out string? known))
                return known;
            string name = new(text[..written]);
            operations.Add(name, name);
            return name;
        }

        public override void Write(Utf8JsonWriter writer, AudioMusicInstructionMetadata value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("operation"u8, value.Operation);
            writer.WriteNumber("opcode"u8, value.Opcode);
            writer.WritePropertyName("timing"u8);
            JsonSerializer.Serialize(writer, value.Timing, options);
            writer.WritePropertyName("arguments"u8);
            JsonSerializer.Serialize(writer, value.Arguments, options);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// Reads every instruction's timing/argument bytes into one exact array instead of a
    /// growing list. Writes the same JSON number array the default collection writer emits.
    /// </summary>
    private sealed class ExactByteListConverter : JsonConverter<IReadOnlyList<byte>>
    {
        public override IReadOnlyList<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected a JSON array of bytes.");
            Span<byte> small = stackalloc byte[16];
            byte[]? rented = null;
            Span<byte> values = small;
            int count = 0;
            try
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (count == values.Length)
                    {
                        byte[] larger = System.Buffers.ArrayPool<byte>.Shared.Rent(values.Length * 2);
                        values.CopyTo(larger);
                        if (rented is not null) System.Buffers.ArrayPool<byte>.Shared.Return(rented);
                        rented = larger;
                        values = larger;
                    }
                    values[count++] = reader.GetByte();
                }
                if (reader.TokenType != JsonTokenType.EndArray)
                    throw new JsonException("Byte array is not terminated.");
                return values[..count].ToArray();
            }
            finally
            {
                if (rented is not null) System.Buffers.ArrayPool<byte>.Shared.Return(rented);
            }
        }

        public override void Write(Utf8JsonWriter writer, IReadOnlyList<byte> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            for (int index = 0; index < value.Count; index++)
                writer.WriteNumberValue(value[index]);
            writer.WriteEndArray();
        }
    }
}
