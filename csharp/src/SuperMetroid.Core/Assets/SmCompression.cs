namespace SuperMetroid.Core.Assets;

/// <summary>
/// Decoder for Super Metroid's command-stream compression format.
/// This is a direct, checked translation of <c>DecompressToMem</c> at <c>$80:B119</c>.
/// </summary>
/// <remarks>
/// A stream is a sequence of commands terminated by <c>$FF</c>. Short command headers
/// encode a 1-32 byte run in one byte. Headers whose upper three bits are all set use a
/// second length byte and can encode runs up to 1024 bytes. The three command bits select
/// literal, fill, alternating-fill, incrementing-fill, absolute-copy, inverted-copy,
/// relative-copy, or inverted-relative-copy behavior.
/// </remarks>
public static class SmCompression
{
    /// <summary>
    /// Decompresses exactly one complete stream. Trailing data is rejected because each
    /// extracted <c>.bin</c> is expected to contain one asset and nothing else.
    /// </summary>
    public static byte[] Decompress(
        ReadOnlySpan<byte> source,
        int maximumOutputBytes = SmCompressionFormat.DefaultMaximumOutputBytes)
    {
        if (!TryDecompress(source, out byte[] output, out int consumed, maximumOutputBytes) || consumed != source.Length)
            throw new InvalidDataException("Input is not one complete Super Metroid compressed stream.");
        return output;
    }

    public static bool TryDecompress(
        ReadOnlySpan<byte> source,
        out byte[] output,
        out int consumed,
        int maximumOutputBytes = SmCompressionFormat.DefaultMaximumOutputBytes)
    {
        // ReadOnlySpan cannot be captured by a local function. Owning this small copy keeps
        // Next() readable and also prevents a caller from mutating input during decoding.
        byte[] inputBytes = source.ToArray();
        var destination = new List<byte>();
        int input = 0;

        // All input reads pass through one bounds-checked cursor. Truncated command streams
        // therefore fail normally instead of leaking IndexOutOfRangeException details.
        bool Next(out byte value)
        {
            if ((uint)input >= (uint)inputBytes.Length)
            {
                value = 0;
                return false;
            }
            value = inputBytes[input++];
            return true;
        }

        // Corrupt backreferences can otherwise create enormous output before eventually
        // failing. The cap is defensive; no known Super Metroid asset approaches 4 MiB.
        bool Append(byte value)
        {
            if (destination.Count >= maximumOutputBytes)
                return false;
            destination.Add(value);
            return true;
        }

        while (Next(out byte header))
        {
            if (header == SmCompressionFormat.Terminator)
            {
                output = destination.ToArray();
                consumed = input;
                return true;
            }

            byte? longLength = null;
            if (SmCompressionFormat.IsLongHeader(header))
            {
                if (!Next(out byte lowLength))
                    break;
                longLength = lowLength;
            }
            SmCompressionHeader decoded = SmCompressionHeader.Decode(header, longLength);

            // Commands 4-7 copy bytes already emitted to the destination. Overlapping
            // copies are intentional and work like LZSS/RLE expansion, one byte at a time.
            if (decoded.IsCopy)
            {
                int copyFrom;
                if (decoded.IsRelativeCopy)
                {
                    // Relative commands encode a one-byte backwards distance.
                    if (!Next(out byte distance) || distance == 0)
                        break;
                    copyFrom = destination.Count - distance;
                }
                else
                {
                    // Absolute commands encode an offset from the start of this output.
                    if (!Next(out byte low) || !Next(out byte high))
                        break;
                    copyFrom = low | (high << 8);
                }

                // Commands 5 and 7 XOR every copied byte with $FF.
                for (int i = 0; i < decoded.Length; i++, copyFrom++)
                {
                    if ((uint)copyFrom >= (uint)destination.Count)
                        goto Invalid;
                    byte value = destination[copyFrom];
                    if (!Append(decoded.InvertsCopiedBytes ? (byte)~value : value))
                        goto Invalid;
                }
                continue;
            }

            switch (decoded.Command)
            {
                case SmCompressionCommand.Literal:
                    for (int i = 0; i < decoded.Length; i++)
                    {
                        if (!Next(out byte value) || !Append(value))
                            goto Invalid;
                    }
                    break;

                case SmCompressionCommand.RepeatByte:
                    if (!Next(out byte repeated))
                        goto Invalid;
                    for (int i = 0; i < decoded.Length; i++)
                        if (!Append(repeated)) goto Invalid;
                    break;

                case SmCompressionCommand.AlternatePair:
                    if (!Next(out byte first) || !Next(out byte second))
                        goto Invalid;
                    for (int i = 0; i < decoded.Length; i++)
                        if (!Append((i & 1) == 0 ? first : second)) goto Invalid;
                    break;

                case SmCompressionCommand.IncrementingSequence:
                    if (!Next(out byte initial))
                        goto Invalid;
                    for (int i = 0; i < decoded.Length; i++)
                        if (!Append((byte)(initial + i))) goto Invalid;
                    break;

                default:
                    goto Invalid;
            }
        }

    Invalid:
        output = [];
        consumed = input;
        return false;
    }
}
