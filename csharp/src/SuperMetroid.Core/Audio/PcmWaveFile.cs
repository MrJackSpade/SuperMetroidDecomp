using System.Buffers.Binary;
using System.Text;

namespace SuperMetroid.Core.Audio;

/// <summary>Strict PCM16 RIFF/WAVE codec: mono for extracted and replacement samples, interleaved stereo for rendered clips (#1305).</summary>
/// <remarks>
/// The single owner of the WAVE file encoding: its two-byte header fields and sixteen-bit samples are read and written
/// at the widths the file format defines.
/// </remarks>
[EncodingOwner("PCM16 RIFF/WAVE file")]
public static class PcmWaveFile
{
    /// <summary>RIFF format code one, required to exclude compressed WAVE encodings.</summary>
    private const ushort PcmFormat = 1;
    /// <summary>PCM's mandatory format fields through bits-per-sample.</summary>
    private const int MinimumFormatChunkBytes = 16;

    /// <summary>Decodes little-endian, uncompressed mono PCM16 after validating the RIFF length and the single format/data chunks; unrelated chunks are skipped.</summary>
    /// <param name="bytes">Complete RIFF/WAVE file bytes, with an even-length data chunk and a positive sample rate.</param>
    /// <param name="sourceName">Nonblank diagnostic label included in validation errors; no file is opened by this method.</param>
    /// <returns>The sampling frequency in hertz and a newly allocated array of signed 16-bit sample amplitudes; loop metadata is not imported.</returns>
    /// <exception cref="InvalidDataException">The container, chunk layout, PCM encoding, or declared size/rate fields are inconsistent.</exception>
    /// <exception cref="OverflowException">A declared length or rate cannot be represented by the checked integer arithmetic.</exception>
    public static PcmSamples ReadMonoPcm16(
        ReadOnlySpan<byte> bytes,
        string sourceName) =>
        ReadPcm16(bytes, sourceName, PcmSampleFormat.ChannelCount);

    /// <summary>Decodes uncompressed interleaved stereo PCM16 with the same validation as <see cref="ReadMonoPcm16"/>.</summary>
    /// <returns>The sampling frequency and the interleaved left/right samples.</returns>
    /// <exception cref="InvalidDataException">The container, chunk layout, PCM encoding, or declared size/rate fields are inconsistent.</exception>
    public static PcmSamples ReadStereoPcm16(ReadOnlySpan<byte> bytes, string sourceName) =>
        ReadPcm16(bytes, sourceName, PcmSampleFormat.StereoChannelCount);

    /// <summary>Decodes uncompressed PCM16 with exactly <paramref name="channelCount"/> interleaved channels.</summary>
    private static PcmSamples ReadPcm16(ReadOnlySpan<byte> bytes, string sourceName, ushort channelCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        if (bytes.Length < 12 || !bytes[..4].SequenceEqual("RIFF"u8) ||
            !bytes.Slice(8, 4).SequenceEqual("WAVE"u8))
        {
            throw new InvalidDataException($"PCM sample '{sourceName}' is not a RIFF/WAVE file.");
        }
        uint riffLength = ReadDword(bytes, 4);
        if (riffLength != bytes.Length - 8)
            throw new InvalidDataException($"PCM sample '{sourceName}' has an inconsistent RIFF length.");

        int sampleRate = 0;
        ReadOnlySpan<byte> sampleBytes = default;
        bool foundFormat = false;
        bool foundData = false;
        for (int offset = 12; offset <= bytes.Length - 8;)
        {
            ReadOnlySpan<byte> id = bytes.Slice(offset, 4);
            int length = checked((int)ReadDword(bytes, offset + 4));
            offset += 8;
            if (length > bytes.Length - offset)
                throw new InvalidDataException($"PCM sample '{sourceName}' contains a truncated RIFF chunk.");
            ReadOnlySpan<byte> chunk = bytes.Slice(offset, length);
            if (id.SequenceEqual("fmt "u8))
            {
                if (foundFormat || length < MinimumFormatChunkBytes)
                    throw new InvalidDataException($"PCM sample '{sourceName}' has an invalid format chunk.");
                ushort format = ReadWord(chunk, 0);
                ushort channels = ReadWord(chunk, 2);
                sampleRate = checked((int)ReadDword(chunk, 4));
                int byteRate = checked((int)ReadDword(chunk, 8));
                ushort blockAlign = ReadWord(chunk, 12);
                ushort bits = ReadWord(chunk, 14);
                if (format != PcmFormat || channels != channelCount ||
                    bits != PcmSampleFormat.BitsPerSample ||
                    blockAlign != channelCount * sizeof(short) || sampleRate <= 0 ||
                    byteRate != checked(sampleRate * channelCount * sizeof(short)))
                {
                    throw new InvalidDataException(
                        $"PCM sample '{sourceName}' must be uncompressed 16-bit PCM with {channelCount} channel(s).");
                }
                foundFormat = true;
            }
            else if (id.SequenceEqual("data"u8))
            {
                if (foundData || length % (channelCount * sizeof(short)) != 0)
                    throw new InvalidDataException($"PCM sample '{sourceName}' has an invalid data chunk.");
                sampleBytes = chunk;
                foundData = true;
            }
            offset = checked(offset + length + (length & 1));
        }
        if (!foundFormat || !foundData)
            throw new InvalidDataException($"PCM sample '{sourceName}' requires one format and one data chunk.");

        short[] samples = new short[sampleBytes.Length / sizeof(short)];
        for (int index = 0; index < samples.Length; index++)
            samples[index] = unchecked((short)ReadWord(sampleBytes, index * sizeof(short)));
        return new PcmSamples(sampleRate, samples);
    }

    /// <summary>Creates or truncates a WAV file and writes a minimal little-endian mono PCM16 RIFF header followed by the supplied samples; no loop chunk is emitted.</summary>
    /// <param name="path">Destination file path; its parent directory must already exist.</param>
    /// <param name="sampleRate">Positive sampling frequency in hertz, written without resampling the input.</param>
    /// <param name="samples">Signed 16-bit amplitudes in playback order; the samples are read but not modified.</param>
    /// <exception cref="ArgumentNullException">The sample collection is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The sample rate is zero or negative.</exception>
    /// <exception cref="OverflowException">The sample count cannot be represented as a signed integer byte length.</exception>
    public static void WriteMonoPcm16(string path, int sampleRate, IReadOnlyList<short> samples) =>
        WritePcm16(path, sampleRate, PcmSampleFormat.ChannelCount, samples);

    /// <summary>Writes interleaved left/right samples as a minimal uncompressed stereo PCM16 WAV file.</summary>
    /// <exception cref="ArgumentException"><paramref name="samples"/> holds an odd count.</exception>
    public static void WriteStereoPcm16(string path, int sampleRate, IReadOnlyList<short> samples) =>
        WritePcm16(path, sampleRate, PcmSampleFormat.StereoChannelCount, samples);

    /// <summary>Writes a minimal PCM16 RIFF header for <paramref name="channelCount"/> interleaved channels, then the samples.</summary>
    private static void WritePcm16(string path, int sampleRate, ushort channelCount, IReadOnlyList<short> samples)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Ensure.NotNull(samples);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        if (samples.Count % channelCount != 0)
            throw new ArgumentException($"{samples.Count} samples do not fill whole {channelCount}-channel frames.", nameof(samples));
        int dataBytes = checked(samples.Count * sizeof(short));
        using FileStream stream = File.Create(path);
        using BinaryWriter writer = new(stream, Encoding.ASCII, leaveOpen: false);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataBytes);
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        writer.Write(MinimumFormatChunkBytes);
        writer.Write(PcmFormat);
        writer.Write(channelCount);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channelCount * sizeof(short));
        writer.Write(unchecked((ushort)(channelCount * sizeof(short))));
        writer.Write(checked((ushort)PcmSampleFormat.BitsPerSample));
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataBytes);
        foreach (short sample in samples)
            writer.Write(sample);
    }

    /// <summary>Decodes a two-byte little-endian RIFF field from a validated chunk span.</summary>
    private static ushort ReadWord(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);

    /// <summary>Decodes a four-byte little-endian RIFF size or sample-rate field.</summary>
    private static uint ReadDword(ReadOnlySpan<byte> bytes, int offset) =>
        System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..]);
}
