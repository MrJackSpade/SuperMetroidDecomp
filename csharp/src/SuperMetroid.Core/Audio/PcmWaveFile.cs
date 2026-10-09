using System.Text;

namespace SuperMetroid.Core.Audio;

/// <summary>Strict mono PCM16 RIFF/WAVE codec used by extracted and replacement samples.</summary>
public static class PcmWaveFile
{
    /// <summary>RIFF format code identifying integer PCM audio.</summary>
    private const ushort PcmFormat = 1;
    /// <summary>Minimum WAVE format payload containing the base PCM fields.</summary>
    private const int MinimumFormatChunkBytes = 16;

    /// <summary>Decodes little-endian, uncompressed mono PCM16 after validating the RIFF length and the single format/data chunks; unrelated chunks are skipped.</summary>
    /// <param name="bytes">Complete RIFF/WAVE file bytes, with an even-length data chunk and a positive sample rate.</param>
    /// <param name="sourceName">Nonblank diagnostic label included in validation errors; no file is opened by this method.</param>
    /// <returns>The sampling frequency in hertz and a newly allocated array of signed 16-bit sample amplitudes; loop metadata is not imported.</returns>
    /// <exception cref="InvalidDataException">The container, chunk layout, PCM encoding, or declared size/rate fields are inconsistent.</exception>
    /// <exception cref="OverflowException">A declared length or rate cannot be represented by the checked integer arithmetic.</exception>
    public static (int SampleRate, short[] Samples) ReadMonoPcm16(
        ReadOnlySpan<byte> bytes,
        string sourceName)
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
                if (format != PcmFormat || channels != PcmSampleFormat.ChannelCount ||
                    bits != PcmSampleFormat.BitsPerSample ||
                    blockAlign != sizeof(short) || sampleRate <= 0 ||
                    byteRate != checked(sampleRate * sizeof(short)))
                {
                    throw new InvalidDataException(
                        $"PCM sample '{sourceName}' must be uncompressed 16-bit mono PCM.");
                }
                foundFormat = true;
            }
            else if (id.SequenceEqual("data"u8))
            {
                if (foundData || (length & 1) != 0)
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
        return (sampleRate, samples);
    }

    /// <summary>Creates or truncates a WAV file and writes a minimal little-endian mono PCM16 RIFF header followed by the supplied samples; no loop chunk is emitted.</summary>
    /// <param name="path">Destination file path; its parent directory must already exist.</param>
    /// <param name="sampleRate">Positive sampling frequency in hertz, written without resampling the input.</param>
    /// <param name="samples">Signed 16-bit amplitudes in playback order; the samples are read but not modified.</param>
    /// <exception cref="ArgumentNullException">The sample collection is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The sample rate is zero or negative.</exception>
    /// <exception cref="OverflowException">The sample count cannot be represented as a signed integer byte length.</exception>
    public static void WriteMonoPcm16(string path, int sampleRate, IReadOnlyList<short> samples)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        int dataBytes = checked(samples.Count * sizeof(short));
        using FileStream stream = File.Create(path);
        using BinaryWriter writer = new(stream, Encoding.ASCII, leaveOpen: false);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataBytes);
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        writer.Write(MinimumFormatChunkBytes);
        writer.Write(PcmFormat);
        writer.Write(PcmSampleFormat.ChannelCount);
        writer.Write(sampleRate);
        writer.Write(sampleRate * sizeof(short));
        writer.Write(unchecked((ushort)sizeof(short)));
        writer.Write(PcmSampleFormat.BitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataBytes);
        foreach (short sample in samples)
            writer.Write(sample);
    }

    /// <summary>Reads one little-endian 16-bit field from a previously bounded chunk.</summary>
    private static ushort ReadWord(ReadOnlySpan<byte> bytes, int offset) =>
        unchecked((ushort)(bytes[offset] | (bytes[offset + 1] << 8)));

    /// <summary>Reads one little-endian 32-bit field from a previously bounded chunk.</summary>
    private static uint ReadDword(ReadOnlySpan<byte> bytes, int offset) =>
        unchecked((uint)(bytes[offset] | (bytes[offset + 1] << 8) |
            (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24)));
}
