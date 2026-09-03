using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// Build-stage extractor for the game's exact SPC upload streams and their inspectable
/// derivatives. The lossless <c>.spcu</c> files drive playback; WAV and JSON files expose
/// BRR samples, instruments, tracks, envelopes, loops, and SFX routing to ordinary tools.
/// </summary>
public static class SpcAudioAssetExtractor
{
    private const int TrackPointerCount = 32;
    private const int InstrumentTableByteLength = 0x100;
    private const int BrrDirectoryAddress = 0x6d00;
    private const int BrrDirectoryEntrySize = 4;
    private const int BrrBlockByteLength = 9;
    private const int BrrSamplesPerBlock = 16;
    private const int MaximumBrrBlocks = ushort.MaxValue / BrrBlockByteLength;
    private const int WavSampleRate = 32_000;

    /// <summary>Extracts all required assets from the repository's private raw directory.</summary>
    public static AudioAssetManifest Extract(string rawDirectory, string audioDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(audioDirectory);
        rawDirectory = Path.GetFullPath(rawDirectory);
        audioDirectory = Path.GetFullPath(audioDirectory);
        if (!Directory.Exists(rawDirectory))
            throw new DirectoryNotFoundException($"Raw asset directory '{rawDirectory}' does not exist.");

        string streamsDirectory = Path.Combine(audioDirectory, "streams");
        string samplesDirectory = Path.Combine(audioDirectory, "samples");
        Directory.CreateDirectory(streamsDirectory);
        Directory.CreateDirectory(samplesDirectory);

        List<AudioUploadManifestEntry> uploads = [];
        Dictionary<int, byte[]> sourceStreams = [];
        foreach (AudioUploadAssetDefinition definition in AudioAssetCatalogData.All)
        {
            string sourcePath = Path.Combine(rawDirectory, definition.Name + ".bin");
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Required raw audio stream '{sourcePath}' is missing.", sourcePath);
            byte[] stream = File.ReadAllBytes(sourcePath);
            ValidateUploadStream(stream, definition.Name);
            string relativePath = Path.Combine("streams", $"{definition.DataIndex:X2}-{definition.Name}.spcu");
            File.WriteAllBytes(Path.Combine(audioDirectory, relativePath), stream);
            sourceStreams.Add(definition.SnesAddress, stream);
            uploads.Add(new AudioUploadManifestEntry(
                definition.Name,
                definition.SnesAddress,
                definition.DataIndex,
                relativePath.Replace('\\', '/'),
                stream.Length,
                Convert.ToHexString(SHA256.HashData(stream))));
        }

        byte[] commonRam = new byte[SpcDriverData.ApuRamSize];
        bool[] commonWritten = new bool[SpcDriverData.ApuRamSize];
        ApplyUpload(commonRam, commonWritten, sourceStreams[AudioAssetCatalogData.Common.SnesAddress]);

        List<AudioBankMetadata> banks = [];
        foreach (AudioUploadAssetDefinition definition in AudioAssetCatalogData.Music)
        {
            byte[] ram = (byte[])commonRam.Clone();
            bool[] written = (bool[])commonWritten.Clone();
            ApplyUpload(ram, written, sourceStreams[definition.SnesAddress]);
            banks.Add(ExtractBankMetadata(definition, audioDirectory, samplesDirectory, ram, written));
        }

        AudioAssetManifest manifest = new(
            AudioAssetManifest.CurrentFormatVersion,
            uploads,
            banks,
            ExtractSoundLibraries());
        File.WriteAllText(
            Path.Combine(audioDirectory, ExtractedAudioAssetCatalog.ManifestFileName),
            JsonSerializer.Serialize(manifest, AudioAssetJson.Options));
        return manifest;
    }

    private static AudioBankMetadata ExtractBankMetadata(
        AudioUploadAssetDefinition definition,
        string audioDirectory,
        string samplesDirectory,
        byte[] ram,
        bool[] written)
    {
        List<ushort> trackPointers = [];
        for (int track = 0; track < TrackPointerCount; track++)
            trackPointers.Add(ReadWord(ram, SpcDriverData.Ram.DefaultMusicPointer + track * 2));
        while (trackPointers.Count != 0 && trackPointers[^1] == 0)
            trackPointers.RemoveAt(trackPointers.Count - 1);

        List<AudioInstrumentMetadata> instruments = [];
        int instrumentCount = InstrumentTableByteLength / SpcDriverData.Ram.InstrumentRecordSize;
        for (int instrument = 0; instrument < instrumentCount; instrument++)
        {
            int address = SpcDriverData.Ram.InstrumentTable +
                instrument * SpcDriverData.Ram.InstrumentRecordSize;
            byte source = ram[address];
            instruments.Add(new AudioInstrumentMetadata(
                instrument,
                (source & 0x80) != 0,
                source,
                ram[address + 1],
                ram[address + 2],
                ram[address + 3],
                unchecked((ushort)((ram[address + 4] << 8) | ram[address + 5]))));
        }

        string bankSampleDirectory = Path.Combine(samplesDirectory, definition.Name);
        Directory.CreateDirectory(bankSampleDirectory);
        List<AudioSampleMetadata> samples = [];
        for (int source = 0; source <= byte.MaxValue; source++)
        {
            int directory = BrrDirectoryAddress + source * BrrDirectoryEntrySize;
            ushort start = ReadWord(ram, directory);
            ushort loop = ReadWord(ram, directory + 2);
            if (start == 0 || !written[start])
                continue;
            if (!TryDecodeBrr(ram, written, start, loop, out short[] pcm, out int blocks, out int? loopSample))
                continue;

            string fileName = $"{source:X2}.wav";
            string absoluteWavPath = Path.Combine(bankSampleDirectory, fileName);
            WritePcm16Wave(absoluteWavPath, pcm);
            samples.Add(new AudioSampleMetadata(
                unchecked((byte)source), start, loop, blocks, loopSample,
                Path.GetRelativePath(audioDirectory, absoluteWavPath).Replace('\\', '/')));
        }

        return new AudioBankMetadata(
            definition.Name, definition.DataIndex, trackPointers, instruments, samples);
    }

    private static List<AudioSoundLibraryMetadata> ExtractSoundLibraries()
    {
        List<AudioSoundLibraryMetadata> libraries = [];
        for (int library = 0; library < SpcSoundEffectTables.StreamPointerTables.Length; library++)
        {
            ushort[] pointers = SpcSoundEffectTables.StreamPointerTables[library];
            byte[] configurations = SpcSoundEffectTables.Configurations[library];
            List<AudioSoundEffectMetadata> effects = [];
            for (int index = 0; index < pointers.Length; index++)
            {
                effects.Add(new AudioSoundEffectMetadata(
                    unchecked((byte)(index + 1)), pointers[index], configurations[index]));
            }
            libraries.Add(new AudioSoundLibraryMetadata(library + 1, effects));
        }
        return libraries;
    }

    private static bool TryDecodeBrr(
        byte[] ram,
        bool[] written,
        ushort start,
        ushort loop,
        out short[] pcm,
        out int blockCount,
        out int? loopSampleIndex)
    {
        List<short> samples = [];
        HashSet<ushort> blockAddresses = [];
        int address = start;
        int old = 0;
        int older = 0;
        bool loops = false;
        blockCount = 0;
        while (blockCount < MaximumBrrBlocks)
        {
            if (address > ushort.MaxValue - BrrBlockByteLength)
                break;
            for (int index = 0; index < BrrBlockByteLength; index++)
            {
                if (!written[address + index])
                    goto Invalid;
            }

            blockAddresses.Add(unchecked((ushort)address));
            byte header = ram[address];
            int shift = header >> 4;
            int filter = (header >> 2) & 3;
            for (int sampleIndex = 0; sampleIndex < BrrSamplesPerBlock; sampleIndex++)
            {
                byte packed = ram[address + 1 + sampleIndex / 2];
                int sample = (sampleIndex & 1) == 0 ? packed >> 4 : packed & 0x0f;
                if (sample > 7)
                    sample -= 16;
                sample = shift <= 0x0c ? (sample << shift) >> 1 : (sample >> 3) << 12;
                sample += filter switch
                {
                    0 => 0,
                    1 => old + (-old >> 4),
                    2 => 2 * old + ((3 * -old) >> 5) - older + (older >> 4),
                    3 => 2 * old + ((13 * -old) >> 6) - older + ((3 * older) >> 4),
                    _ => throw new InvalidDataException($"Impossible BRR filter {filter}."),
                };
                sample = Math.Clamp(sample, short.MinValue, short.MaxValue);
                sample = unchecked((short)((sample & 0x7fff) << 1)) >> 1;
                older = old;
                old = sample;
                samples.Add(unchecked((short)sample));
            }
            address += BrrBlockByteLength;
            blockCount++;
            if ((header & 1) != 0)
            {
                loops = (header & 2) != 0;
                loopSampleIndex = loops && blockAddresses.Contains(loop)
                    ? ((loop - start) / BrrBlockByteLength) * BrrSamplesPerBlock
                    : null;
                pcm = [.. samples];
                return !loops || loopSampleIndex is not null;
            }
        }

    Invalid:
        pcm = [];
        blockCount = 0;
        loopSampleIndex = null;
        return false;
    }

    private static void WritePcm16Wave(string path, short[] samples)
    {
        int dataBytes = checked(samples.Length * sizeof(short));
        using FileStream stream = File.Create(path);
        using BinaryWriter writer = new(stream, Encoding.ASCII, leaveOpen: false);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataBytes);
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        writer.Write(16);
        writer.Write(unchecked((ushort)1));
        writer.Write(unchecked((ushort)1));
        writer.Write(WavSampleRate);
        writer.Write(WavSampleRate * sizeof(short));
        writer.Write(unchecked((ushort)sizeof(short)));
        writer.Write(unchecked((ushort)16));
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataBytes);
        foreach (short sample in samples)
            writer.Write(sample);
    }

    private static void ValidateUploadStream(ReadOnlySpan<byte> stream, string name)
    {
        byte[] scratch = new byte[SpcDriverData.ApuRamSize];
        bool[] written = new bool[SpcDriverData.ApuRamSize];
        try
        {
            ApplyUpload(scratch, written, stream);
        }
        catch (InvalidDataException exception)
        {
            throw new InvalidDataException($"Raw audio asset '{name}' is not a valid SPC upload stream.", exception);
        }
    }

    private static void ApplyUpload(byte[] ram, bool[] written, ReadOnlySpan<byte> stream)
    {
        int offset = 0;
        for (;;)
        {
            if (offset > stream.Length - 2)
                throw new InvalidDataException("SPC upload ended before its terminating length word.");
            ushort length = unchecked((ushort)(stream[offset] | (stream[offset + 1] << 8)));
            offset += 2;
            if (length == 0)
            {
                // Retail assets retain the two-byte SPC execution address consumed by the
                // upload handshake after its zero-length marker. The managed driver already
                // runs, so playback ignores it while extraction preserves it byte-for-byte.
                if (stream.Length - offset is not 0 and not 2)
                    throw new InvalidDataException($"SPC upload has {stream.Length - offset} unexpected bytes after its terminator.");
                return;
            }
            if (offset > stream.Length - 2)
                throw new InvalidDataException("SPC upload ended before a destination word.");
            ushort destination = unchecked((ushort)(stream[offset] | (stream[offset + 1] << 8)));
            offset += 2;
            if (length > stream.Length - offset)
                throw new InvalidDataException($"SPC upload record ${destination:X4} overruns its source stream.");
            if (destination + length > ram.Length)
                throw new InvalidDataException($"SPC upload record ${destination:X4}+${length:X4} overruns APU RAM.");
            stream.Slice(offset, length).CopyTo(ram.AsSpan(destination, length));
            Array.Fill(written, true, destination, length);
            offset += length;
        }
    }

    private static ushort ReadWord(byte[] bytes, int address) =>
        unchecked((ushort)(bytes[address] | (bytes[address + 1] << 8)));
}
