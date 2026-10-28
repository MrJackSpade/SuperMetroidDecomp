using System.Text.Json;
using System.Security.Cryptography;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Immutable collection of extracted SPC upload streams keyed by <see cref="AudioBank"/>.
/// The on-disk manifest still names each upload by its cartridge address; loading maps it to
/// the bank identity once, so runtime audio reads no music or sample bytes from the ROM.
/// </summary>
public sealed class ExtractedAudioAssetCatalog
{
    /// <summary>JSON manifest filename beneath the selected audio root, identifying upload streams, canonical WAV samples, and authored bank/SFX definitions.</summary>
    public const string ManifestFileName = "audio-manifest.json";
    /// <summary>Opaque SPC upload bytes indexed by their original cartridge command address.</summary>
    private readonly IReadOnlyDictionary<AudioBank, byte[]> streams;
    /// <summary>Canonical replaceable PCM samples associated with each upload address.</summary>
    internal readonly IReadOnlyDictionary<AudioBank, ManagedPcmSampleBank> sampleBanks;
    /// <summary>Validated six-byte instrument definitions selected by each music upload.</summary>
    private readonly IReadOnlyDictionary<AudioBank, IReadOnlyList<AudioInstrumentMetadata>>
        instrumentBanks;
    /// <summary>Compiled track, phrase, and channel definitions for each music upload.</summary>
    private readonly IReadOnlyDictionary<AudioBank, AudioBankMetadata> musicBanks;
    /// <summary>Number of distinct PCM assets after equivalent sample sources are aliased.</summary>
    internal readonly int canonicalSampleCount;

    /// <summary>Retains the validated stream, sample, instrument, music, and SFX catalogs as one selected installation.</summary>
    private ExtractedAudioAssetCatalog(ExtractedAudioAssetParts parts)
    {
        this.streams = parts.Streams;
        this.sampleBanks = parts.SampleBanks;
        this.instrumentBanks = parts.InstrumentBanks;
        this.musicBanks = parts.MusicBanks;
        SoundPrograms = parts.SoundPrograms;
        SoundLibraries = parts.SoundLibraries;
        this.canonicalSampleCount = parts.CanonicalSampleCount;
        ContentIdentity = parts.ContentIdentity;
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

        Dictionary<AudioBank, byte[]> loaded = [];
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
            AudioBank uploadBank = AudioAssetCatalogDefinitions.ResolveSnesAddress(entry.SnesAddress).AudioSet;
            if (!loaded.TryAdd(uploadBank, bytes))
                throw new InvalidDataException($"Audio manifest repeats SNES upload address ${entry.SnesAddress:X6}.");
        }

        int[] missing = AudioAssetCatalogDefinitions.All
            .Where(definition => !loaded.ContainsKey(definition.AudioSet))
            .Select(definition => definition.SnesAddress)
            .ToArray();
        if (missing.Length != 0)
        {
            throw new InvalidDataException(
                $"Audio manifest omits required upload addresses: {string.Join(", ", missing.Select(address => $"${address:X6}"))}.");
        }
        Dictionary<string, ManagedPcmSample> canonicalSamples =
            LoadCanonicalSamples(root, manifest.CanonicalSamples);
        Dictionary<AudioBank, ManagedPcmSampleBank> banks = [];
        Dictionary<AudioBank, IReadOnlyList<AudioInstrumentMetadata>> instruments = [];
        Dictionary<AudioBank, AudioBankMetadata> musicBanks = [];
        foreach (AudioBankMetadata bank in manifest.Banks)
        {
            AudioUploadAssetDefinition definition = AudioAssetCatalogDefinitions.All.SingleOrDefault(
                candidate => candidate.SnesAddress == bank.SnesAddress)
                ?? throw new InvalidDataException(
                    $"Audio manifest contains unknown PCM bank address ${bank.SnesAddress:X6}.");
            if (bank.Name != definition.Name || bank.DataIndex != definition.DataIndex)
            {
                throw new InvalidDataException(
                    $"PCM bank ${bank.SnesAddress:X6} identity '{bank.Name}'/${bank.DataIndex:X2} " +
                    $"does not match '{definition.Name}'/${definition.DataIndex:X2}.");
            }
            Dictionary<int, ManagedPcmSample> sources = [];
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
            Dictionary<int, int> loopEntries = [];
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
            if (!banks.TryAdd(definition.AudioSet, new ManagedPcmSampleBank(bank.Name, definition.AudioSet, sources, loopEntries)))
                throw new InvalidDataException($"Audio manifest repeats sample bank address ${bank.SnesAddress:X6}.");
            if (!instruments.TryAdd(definition.AudioSet, ValidateInstruments(bank)))
                throw new InvalidDataException($"Audio manifest repeats instrument bank address ${bank.SnesAddress:X6}.");
            ValidateMusicBank(bank);
            if (!musicBanks.TryAdd(definition.AudioSet, bank))
                throw new InvalidDataException($"Audio manifest repeats music bank address ${bank.SnesAddress:X6}.");
        }
        int[] missingBanks = AudioAssetCatalogDefinitions.All
            .Where(definition => !banks.ContainsKey(definition.AudioSet))
            .Select(definition => definition.SnesAddress)
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
        return new ExtractedAudioAssetCatalog(new ExtractedAudioAssetParts { Streams = loaded, SampleBanks = banks, InstrumentBanks = instruments, MusicBanks = musicBanks, SoundPrograms = soundPrograms, SoundLibraries = soundLibraries, CanonicalSampleCount = canonicalSamples.Count, ContentIdentity = ComputeContentIdentity(manifest) });
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
        /// <summary>Canonical manifest hashing never reads the bytes it receives.</summary>
        public override bool CanRead => false;
        /// <summary>The digest consumes JSON in write order without a seekable buffer.</summary>
        public override bool CanSeek => false;
        /// <summary>Accepts serialized manifest bytes for the incremental digest.</summary>
        public override bool CanWrite => true;
        /// <summary>Length is unavailable because no manifest byte buffer is retained.</summary>
        public override long Length => throw new NotSupportedException();
        /// <summary>Seeking is unavailable on the incremental hash sink.</summary>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>Feeds an array segment directly into the canonical manifest hash.</summary>
        public override void Write(byte[] buffer, int offset, int count) => hash.AppendData(buffer, offset, count);
        /// <summary>Feeds a byte span directly into the canonical manifest hash.</summary>
        public override void Write(ReadOnlySpan<byte> buffer) => hash.AppendData(buffer);
        /// <summary>Completes a stream flush without changing the incremental hash state.</summary>
        public override void Flush() { }
        /// <summary>Rejects attempts to read from the write-only hash sink.</summary>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        /// <summary>Rejects seeking because the hash reflects serialized write order.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>Rejects resizing because serialized bytes are consumed immediately.</summary>
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    /// <summary>Returns the exact terminated upload stream of one bank.</summary>
    /// <exception cref="InvalidDataException"><paramref name="bank"/> is not a loaded bank.</exception>
    public ReadOnlyMemory<byte> GetUpload(AudioBank bank) =>
        streams.TryGetValue(bank, out byte[]? bytes)
            ? bytes
            : throw new InvalidDataException($"Audio references unmapped extracted upload {bank}.");

    /// <summary>Returns the replaceable PCM source map installed by the same upload.</summary>
    /// <exception cref="InvalidDataException"><paramref name="bank"/> is not a loaded bank.</exception>
    public ManagedPcmSampleBank GetSampleBank(AudioBank bank) =>
        sampleBanks.TryGetValue(bank, out ManagedPcmSampleBank? samples)
            ? samples
            : throw new InvalidDataException($"Audio references unmapped PCM bank {bank}.");

    /// <summary>
    /// Returns the editable instrument table installed after the corresponding opaque
    /// upload. The ordered records retain the SPC driver's native six-byte identities.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="bank"/> is not a loaded bank.</exception>
    public IReadOnlyList<AudioInstrumentMetadata> GetInstrumentBank(AudioBank bank) =>
        instrumentBanks.TryGetValue(bank, out IReadOnlyList<AudioInstrumentMetadata>? instruments)
            ? instruments
            : throw new InvalidDataException($"Audio references unmapped instrument bank {bank}.");

    /// <summary>Returns the decoded authored music definitions installed by one upload.</summary>
    /// <exception cref="InvalidDataException"><paramref name="bank"/> is not a loaded bank.</exception>
    public AudioBankMetadata GetMusicBank(AudioBank bank) =>
        musicBanks.TryGetValue(bank, out AudioBankMetadata? music)
            ? music
            : throw new InvalidDataException($"Audio references unmapped music bank {bank}.");

    /// <summary>Decoded, address-stable authored SFX channel programs.</summary>
    public IReadOnlyList<AudioSoundProgramMetadata> SoundPrograms { get; }

    /// <summary>Stable SFX command IDs and their compiled routing/allocation metadata.</summary>
    public IReadOnlyList<AudioSoundLibraryMetadata> SoundLibraries { get; }

    /// <summary>Orders every required instrument by native index and rejects duplicates or inconsistent noise markers.</summary>
    private static System.Collections.ObjectModel.ReadOnlyCollection<AudioInstrumentMetadata>
        ValidateInstruments(
        AudioBankMetadata bank)
    {
        if (bank.Instruments.Count != SpcDriverDefinitions.Ram.InstrumentCount)
        {
            throw new InvalidDataException(
                $"Audio bank '{bank.Name}' defines {bank.Instruments.Count} instruments; " +
                $"expected {SpcDriverDefinitions.Ram.InstrumentCount}.");
        }

        Dictionary<int, AudioInstrumentMetadata> byId = [];
        foreach (AudioInstrumentMetadata instrument in bank.Instruments)
        {
            if ((uint)instrument.Instrument >= SpcDriverDefinitions.Ram.InstrumentCount)
            {
                throw new InvalidDataException(
                    $"Audio bank '{bank.Name}' instrument {instrument.Instrument} is outside " +
                    $"0..{SpcDriverDefinitions.Ram.InstrumentCount - 1}.");
            }
            if (instrument.UsesNoise !=
                ((instrument.SourceOrNoiseRate & SpcDriverDefinitions.Instruments.NoiseMarker) != 0))
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

        var ordered = new AudioInstrumentMetadata[SpcDriverDefinitions.Ram.InstrumentCount];
        for (int instrument = 0; instrument < ordered.Length; instrument++)
        {
            if (!byId.TryGetValue(instrument, out AudioInstrumentMetadata? definition))
                throw new InvalidDataException($"Audio bank '{bank.Name}' omits instrument {instrument}.");
            ordered[instrument] = definition;
        }
        return Array.AsReadOnly(ordered);
    }

    /// <summary>Loads hashed mono PCM WAVs, validates rate and length, and rejects duplicate decoded sample/loop pairs.</summary>
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

    /// <summary>Checks stable track, phrase, and program addresses before compiling an authored SPC music bank.</summary>
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
        Dictionary<int, AudioMusicPhraseMetadata> phrases = [];
        foreach (AudioMusicPhraseMetadata phrase in bank.MusicPhrases)
        {
            string expectedId = $"music-{bank.DataIndex:x2}-phrase-{phrase.Address:x4}";
            if (phrase.Id != expectedId || !phrases.TryAdd(phrase.Address, phrase))
                throw new InvalidDataException($"Audio bank '{bank.Name}' has invalid phrase identity '{phrase.Id}'.");
        }
        Dictionary<int, AudioMusicProgramMetadata> programs = [];
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
                ushort target = unchecked(
                    LittleEndianBytes.Word(instruction.Arguments[0], instruction.Arguments[1]));
                if (!programs.ContainsKey(target))
                {
                    throw new InvalidDataException(
                        $"Music program '{program.Id}' calls missing program ${target:X4}.");
                }
            }
        }
        _ = SpcMusicDefinitionCodec.CompileBank(bank);
    }

    /// <summary>Validates bounded SFX programs and each library's fixed command routing and voice allocation.</summary>
    private static SoundDefinitionMetadata ValidateSoundDefinitions(
        AudioAssetManifest manifest)
    {
        if (manifest.SoundPrograms is null)
            throw new InvalidDataException("Audio manifest has no decoded sound-program catalog.");
        if (manifest.SoundLibraries is null)
            throw new InvalidDataException("Audio manifest has no sound-library catalog.");
        Dictionary<string, AudioSoundProgramMetadata> programsById = new(StringComparer.Ordinal);
        HashSet<int> addresses = [];
        foreach (AudioSoundProgramMetadata program in manifest.SoundPrograms)
        {
            if (string.IsNullOrWhiteSpace(program.Id))
                throw new InvalidDataException("Audio manifest contains a sound program with no stable ID.");
            if (!programsById.TryAdd(program.Id, program))
                throw new InvalidDataException($"Audio manifest repeats sound program ID '{program.Id}'.");
            if (!addresses.Add(program.Address))
                throw new InvalidDataException($"Audio manifest repeats sound program address ${program.Address:X4}.");
            if (program.ByteCapacity <= 0 || program.Address + program.ByteCapacity > SpcDriverDefinitions.ApuRamSize)
            {
                throw new InvalidDataException(
                    $"Audio sound program '{program.Id}' has invalid fixed slot " +
                    $"${program.Address:X4}+{program.ByteCapacity}.");
            }
            _ = SpcSoundEffectProgramCodec.Encode(program);
        }

        if (manifest.SoundLibraries.Count != SpcDriverDefinitions.SoundEffects.LibraryCount)
        {
            throw new InvalidDataException(
                $"Audio manifest defines {manifest.SoundLibraries.Count} SFX libraries; " +
                $"expected {SpcDriverDefinitions.SoundEffects.LibraryCount}.");
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
        return new SoundDefinitionMetadata(Array.AsReadOnly(manifest.SoundPrograms.ToArray()),
            Array.AsReadOnly(manifest.SoundLibraries.ToArray()));
    }

    /// <summary>Resolves an audio manifest resource beneath the installation root and rejects escapes or missing files.</summary>
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

}
