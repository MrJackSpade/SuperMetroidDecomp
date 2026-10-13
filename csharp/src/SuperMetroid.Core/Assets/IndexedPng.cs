using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using SuperMetroid.Core.Spatial;

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
    /// <summary>Decodes a complete static indexed PNG, preserving each pixel's palette index and palette alpha; verifies dimensions, chunk ordering/checksums, the 16-MiB encoded limit, and exact inflated row length before returning unpacked pixels.</summary>
    /// <param name="input">Caller-owned stream at the PNG signature, left open; the image must end at stream EOF without trailing bytes.</param>
    /// <param name="expectedSize">Required pixel width and height, each 1..2048; the file must match exactly.</param>
    /// <returns>New row-major one-byte palette indices and the decoded RGB/alpha palette, before SNES bitplane encoding or color assignment.</returns>
    /// <exception cref="ArgumentNullException">The input stream is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A required dimension is outside 1..2048.</exception>
    /// <exception cref="InvalidDataException">The PNG is malformed, truncated, oversized, has different dimensions, or uses unsupported truecolor, animation, or interlace.</exception>
    public static IndexedPngImage Read(Stream input, Extent16<ImagePixels> expectedSize)
    {
        Ensure.NotNull(input);
        ValidateDimensions(expectedSize);
        try { return ReadCore(input, expectedSize); }
        catch (EndOfStreamException error) { throw new InvalidDataException("Indexed PNG is truncated.", error); }
    }

    /// <summary>Checks PNG chunk framing, CRCs, palette order, transparency, and format before inflating indexed rows.</summary>
    private static IndexedPngImage ReadCore(Stream input, Extent16<ImagePixels> size)
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
            uint length = BinaryPrimitives.ReadUInt32BigEndian(prefix);
            if (length > IndexedPngFormat.MaximumEncodedBytes ||
                encodedBytes > IndexedPngFormat.MaximumEncodedBytes - (long)length - 12)
                throw new InvalidDataException("Indexed PNG exceeds the encoded asset limit.");
            encodedBytes += checked((int)length + 12);
            byte[] payload = new byte[(int)length];
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
                    if (BinaryPrimitives.ReadUInt32BigEndian(payload) != size.X ||
                        BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(4)) != size.Y)
                        throw new InvalidDataException($"PNG must be exactly {size.X} by {size.Y} pixels.");
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
                    if (!dataSeen || palette is null || payload.Length != 0 || input.ReadByte() != -1)
                        throw new InvalidDataException("Invalid PNG end or trailing data.");
                    return DecodeRows(compressed, size, depth, palette);
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

    /// <summary>Reverses PNG row filters and unpacks 1-, 2-, 4-, or 8-bit indices within the exact expected image extent.</summary>
    private static IndexedPngImage DecodeRows(MemoryStream compressed, Extent16<ImagePixels> size, int depth, Rgba32[] palette)
    {
        compressed.Position = 0;
        using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
        int stride = (size.X * depth + 7) / 8;
        byte[] current = new byte[stride], previous = new byte[stride], pixels = new byte[size.Area];
        for (int y = 0; y < size.Y; y++)
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
            for (int x = 0; x < size.X; x++)
            {
                int bit = x * depth;
                byte index = (byte)((current[bit / 8] >> (8 - depth - bit % 8)) & ((1 << depth) - 1));
                if (index >= palette.Length) throw new InvalidDataException("PNG pixel references a missing palette entry.");
                pixels[size.RowMajorIndex(new(checked((ushort)x), checked((ushort)y)))] = index;
            }
            (previous, current) = (current, previous);
        }
        if (zlib.ReadByte() != -1) throw new InvalidDataException("PNG contains excess decompressed image data.");
        return new(size, pixels, palette);
    }

    /// <summary>Writes a noninterlaced eight-bit indexed PNG with unfiltered rows, preserving index identity and palette RGB/alpha exactly; a transparency chunk is emitted only when some palette alpha is not 255.</summary>
    /// <param name="output">Caller-owned destination stream at its current position, left open; existing trailing bytes are not truncated.</param>
    /// <param name="size">Pixel width and height, each 1..2048.</param>
    /// <param name="pixels">Exactly width times height row-major palette indices, one byte per pixel; every index must address an entry in the supplied palette.</param>
    /// <param name="palette">One through 256 ordered RGBA entries; no color matching, quantization, or palette-index reassignment is performed.</param>
    /// <exception cref="ArgumentNullException">The output stream or palette is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is outside 1..2048.</exception>
    /// <exception cref="ArgumentException">Pixel count, palette count, or a pixel's palette index is invalid.</exception>
    public static void Write(Stream output, Extent16<ImagePixels> size, ReadOnlySpan<byte> pixels, IReadOnlyList<Rgba32> palette)
    {
        Ensure.NotNull(output);
        Ensure.NotNull(palette);
        ValidateDimensions(size);
        if (pixels.Length != size.Area || palette.Count is < 1 or > 256)
            throw new ArgumentException("Indexed PNG pixels or palette do not match its dimensions.");
        foreach (byte index in pixels)
            if (index >= palette.Count) throw new ArgumentException("Indexed PNG pixel exceeds palette.");
        output.Write(IndexedPngFormat.Signature);
        byte[] header = new byte[IndexedPngFormat.HeaderLength];
        BinaryPrimitives.WriteUInt32BigEndian(header, (uint)size.X);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), (uint)size.Y);
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
            for (int y = 0; y < size.Y; y++) { zlib.WriteByte(0); zlib.Write(pixels.Slice(y * size.X, size.X)); }
        }
        PngWriter.WriteChunk(output, "IDAT", compressed.ToArray());
        PngWriter.WriteChunk(output, "IEND", []);
    }

    /// <summary>Chooses the nearest left, upper, or upper-left byte to the PNG Paeth predictor, with native tie order.</summary>
    private static int Paeth(int a, int b, int c)
    {
        int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    /// <summary>Bounds each image dimension to 1..2048 before decoding or writing pixel buffers.</summary>
    private static void ValidateDimensions(Extent16<ImagePixels> size)
    {
        if (size.X < 1 || size.Y < 1 ||
            size.X > IndexedPngFormat.MaximumDimension || size.Y > IndexedPngFormat.MaximumDimension)
            throw new ArgumentOutOfRangeException(nameof(size), "Indexed PNG dimensions must be between 1 and 2048.");
    }
}

/// <summary>Decoded source indexes and palette, before any SNES tile encoding or palette assignment.</summary>
/// <param name="Size">Image width and height in pixels.</param>
/// <param name="Pixels">Row-major unpacked palette indices, one byte per pixel; the array is exposed directly rather than copied by the record.</param>
/// <param name="Palette">Ordered RGB/alpha entries addressed by the pixel indices; the array is exposed directly and is not itself a SNES CGRAM palette.</param>
public sealed record IndexedPngImage(Extent16<ImagePixels> Size, byte[] Pixels, Rgba32[] Palette);
