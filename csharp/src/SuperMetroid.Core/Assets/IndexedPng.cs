using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace SuperMetroid.Core.Assets;

/// <summary>Indexed PNG asset codec preserving palette indexes instead of matching expanded RGB colors.</summary>
/// <remarks>
/// Supports noninterlaced indexed PNG at 1/2/4/8 bits and all five scanline filters.
/// Rejects truecolor, animation and interlace explicitly. Caller-specified dimensions
/// are checked before decompression; encoded and inflated data are bounded.
/// See https://www.w3.org/TR/png-3/ sections 9 and 11.2.
/// </remarks>
public static class IndexedPng
{
    public static IndexedPngImage Read(Stream input, int expectedWidth, int expectedHeight)
    {
        ArgumentNullException.ThrowIfNull(input);
        ValidateDimensions(expectedWidth, expectedHeight);
        try { return ReadCore(input, expectedWidth, expectedHeight); }
        catch (EndOfStreamException error) { throw new InvalidDataException("Indexed PNG is truncated.", error); }
    }

    private static IndexedPngImage ReadCore(Stream input, int width, int height)
    {
        Span<byte> signature = stackalloc byte[8];
        input.ReadExactly(signature);
        if (!signature.SequenceEqual(IndexedPngFormat.Signature)) throw new InvalidDataException("Invalid PNG signature.");
        bool headerSeen = false, dataSeen = false, dataEnded = false, transparencySeen = false;
        int depth = 0, encodedBytes = signature.Length;
        Rgba32[]? palette = null;
        using var compressed = new MemoryStream();
        while (true)
        {
            byte[] prefix = new byte[8];
            input.ReadExactly(prefix);
            foreach (byte letter in prefix.AsSpan(4))
                if (letter is not (>= (byte)'A' and <= (byte)'Z') and not (>= (byte)'a' and <= (byte)'z'))
                    throw new InvalidDataException("PNG chunk type must contain ASCII letters.");
            if (prefix[6] is >= (byte)'a' and <= (byte)'z')
                throw new InvalidDataException("PNG chunk type uses a reserved lowercase bit.");
            uint size = BinaryPrimitives.ReadUInt32BigEndian(prefix);
            if (size > IndexedPngFormat.MaximumEncodedBytes ||
                encodedBytes > IndexedPngFormat.MaximumEncodedBytes - (long)size - 12)
                throw new InvalidDataException("Indexed PNG exceeds the encoded asset limit.");
            encodedBytes += checked((int)size + 12);
            byte[] payload = new byte[(int)size];
            input.ReadExactly(payload);
            byte[] checksum = new byte[4];
            input.ReadExactly(checksum);
            uint crc = IndexedPngFormat.InitialCrc;
            foreach (byte value in prefix.AsSpan(4)) crc = PngWriter.UpdateCrc(crc, value);
            foreach (byte value in payload) crc = PngWriter.UpdateCrc(crc, value);
            string name = Encoding.ASCII.GetString(prefix, 4, 4);
            if (~crc != BinaryPrimitives.ReadUInt32BigEndian(checksum))
                throw new InvalidDataException($"PNG {name} checksum mismatch.");
            if (!headerSeen && name != "IHDR") throw new InvalidDataException("PNG must start with IHDR.");
            if (dataSeen && name != "IDAT") dataEnded = true;
            switch (name)
            {
                case "IHDR":
                    if (headerSeen || payload.Length != IndexedPngFormat.HeaderLength)
                        throw new InvalidDataException("Invalid or repeated PNG header.");
                    if (BinaryPrimitives.ReadUInt32BigEndian(payload) != width ||
                        BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(4)) != height)
                        throw new InvalidDataException($"PNG must be exactly {width} by {height} pixels.");
                    depth = payload[8];
                    if (depth is not (1 or 2 or 4 or 8) || payload[9] != IndexedPngFormat.IndexedColorType ||
                        payload[10] != 0 || payload[11] != 0 || payload[12] != 0)
                        throw new InvalidDataException("Asset requires noninterlaced indexed PNG (1, 2, 4 or 8 bits).");
                    headerSeen = true;
                    break;
                case "PLTE":
                    if (palette is not null || dataSeen || payload.Length == 0 || payload.Length % 3 != 0 || payload.Length / 3 > 1 << depth)
                        throw new InvalidDataException("Invalid PNG palette size, order or duplicate.");
                    palette = Enumerable.Range(0, payload.Length / 3)
                        .Select(i => new Rgba32(payload[i * 3], payload[i * 3 + 1], payload[i * 3 + 2], 255)).ToArray();
                    break;
                case "tRNS":
                    if (palette is null || dataSeen || transparencySeen || payload.Length > palette.Length)
                        throw new InvalidDataException("Invalid PNG transparency size, order or duplicate.");
                    for (int i = 0; i < payload.Length; i++) palette[i] = palette[i] with { A = payload[i] };
                    transparencySeen = true;
                    break;
                case "IDAT":
                    if (palette is null || dataEnded) throw new InvalidDataException("PNG image data must follow its palette in consecutive chunks.");
                    dataSeen = true;
                    compressed.Write(payload);
                    break;
                case "IEND":
                    if (!dataSeen || payload.Length != 0 || input.ReadByte() != -1)
                        throw new InvalidDataException("Invalid PNG end or trailing data.");
                    return DecodeRows(compressed, width, height, depth, palette!);
                case "acTL": case "fcTL": case "fdAT":
                    throw new InvalidDataException("Animated PNG is not supported for a static tile atlas.");
                default:
                    // Ancillary metadata is not a pixel operation. Unknown critical
                    // chunks cannot safely be ignored by an asset loader.
                    if ((prefix[4] & 32) == 0) throw new InvalidDataException($"Unsupported critical PNG chunk {name}.");
                    break;
            }
        }
    }

    private static IndexedPngImage DecodeRows(MemoryStream compressed, int width, int height, int depth, Rgba32[] palette)
    {
        compressed.Position = 0;
        using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
        int stride = (width * depth + 7) / 8;
        byte[] current = new byte[stride], previous = new byte[stride], pixels = new byte[width * height];
        for (int y = 0; y < height; y++)
        {
            int filter = zlib.ReadByte();
            if (filter < 0) throw new EndOfStreamException();
            if (filter > (int)PngRowFilter.Paeth) throw new InvalidDataException($"Invalid PNG row filter {filter}.");
            zlib.ReadExactly(current);
            for (int i = 0; i < stride; i++)
            {
                int left = i == 0 ? 0 : current[i - 1], up = previous[i], upperLeft = i == 0 ? 0 : previous[i - 1];
                int prediction = (PngRowFilter)filter switch
                {
                    PngRowFilter.None => 0, PngRowFilter.Sub => left, PngRowFilter.Up => up,
                    PngRowFilter.Average => (left + up) / 2, PngRowFilter.Paeth => Paeth(left, up, upperLeft),
                    _ => throw new InvalidDataException("Unsupported PNG predictor.")
                };
                current[i] = unchecked((byte)(current[i] + prediction));
            }
            for (int x = 0; x < width; x++)
            {
                int bit = x * depth;
                byte index = (byte)((current[bit / 8] >> (8 - depth - bit % 8)) & ((1 << depth) - 1));
                if (index >= palette.Length) throw new InvalidDataException("PNG pixel references a missing palette entry.");
                pixels[y * width + x] = index;
            }
            (previous, current) = (current, previous);
        }
        if (zlib.ReadByte() != -1) throw new InvalidDataException("PNG contains excess decompressed image data.");
        return new(width, height, pixels, palette);
    }

    public static void Write(Stream output, int width, int height, ReadOnlySpan<byte> pixels, IReadOnlyList<Rgba32> palette)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(palette);
        ValidateDimensions(width, height);
        if (pixels.Length != width * height || palette.Count is < 1 or > 256)
            throw new ArgumentException("Indexed PNG pixels or palette do not match its dimensions.");
        foreach (byte index in pixels)
            if (index >= palette.Count) throw new ArgumentException("Indexed PNG pixel exceeds palette.");
        output.Write(IndexedPngFormat.Signature);
        byte[] header = new byte[IndexedPngFormat.HeaderLength];
        BinaryPrimitives.WriteUInt32BigEndian(header, (uint)width);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), (uint)height);
        header[8] = 8;
        header[9] = IndexedPngFormat.IndexedColorType;
        PngWriter.WriteChunk(output, "IHDR", header);
        byte[] colors = new byte[palette.Count * 3], alpha = new byte[palette.Count];
        for (int i = 0; i < palette.Count; i++)
        {
            colors[i * 3] = palette[i].R; colors[i * 3 + 1] = palette[i].G; colors[i * 3 + 2] = palette[i].B;
            alpha[i] = palette[i].A;
        }
        PngWriter.WriteChunk(output, "PLTE", colors);
        if (alpha.Any(a => a != 255)) PngWriter.WriteChunk(output, "tRNS", alpha);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            for (int y = 0; y < height; y++) { zlib.WriteByte(0); zlib.Write(pixels.Slice(y * width, width)); }
        }
        PngWriter.WriteChunk(output, "IDAT", compressed.ToArray());
        PngWriter.WriteChunk(output, "IEND", []);
    }

    private static int Paeth(int a, int b, int c)
    {
        int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    private static void ValidateDimensions(int width, int height)
    {
        if (width < 1 || height < 1 || width > IndexedPngFormat.MaximumDimension || height > IndexedPngFormat.MaximumDimension)
            throw new ArgumentOutOfRangeException(nameof(width), "Indexed PNG dimensions must be between 1 and 2048.");
    }
}

/// <summary>Decoded source indexes and palette, before any SNES tile encoding or palette assignment.</summary>
public sealed record IndexedPngImage(int Width, int Height, byte[] Pixels, Rgba32[] Palette);
