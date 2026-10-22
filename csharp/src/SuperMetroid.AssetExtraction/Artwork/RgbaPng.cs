using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Reads and writes the installation's pre-coloured artwork: 8-bit RGBA PNG images, non-interlaced. A pixel with zero
/// alpha is transparent. Anything else (indexed or greyscale colour, other bit depths, interlacing) is rejected, so an
/// edited image is saved in the same form as the stock one.
/// </summary>
public static class RgbaPng
{
    /// <summary>The eight-byte PNG signature.</summary>
    private static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];

    /// <summary>The colour type of 8-bit-per-channel RGBA.</summary>
    private const byte RgbaColourType = 6;

    /// <summary>Bytes per RGBA pixel.</summary>
    private const int PixelBytes = 4;

    /// <summary>The largest dimension accepted, which bounds a malformed header's allocation.</summary>
    private const int MaximumDimension = 8192;

    /// <summary>Writes an RGBA image as a PNG.</summary>
    /// <param name="output">The stream written to; the caller keeps ownership.</param>
    /// <param name="size">The image's width and height; both are positive.</param>
    /// <param name="pixels">Width times height pixels, row by row from the top.</param>
    /// <exception cref="ArgumentException">A dimension is zero or the pixel count does not match.</exception>
    public static void Write(Stream output, Extent16<ImagePixels> size, ReadOnlySpan<Rgba32> pixels)
    {
        Ensure.NotNull(output);
        if (size.X == 0 || size.Y == 0 || pixels.Length != size.X * size.Y)
            throw new ArgumentException($"A {size.X}x{size.Y} image needs {size.X * size.Y} pixels, not {pixels.Length}.", nameof(pixels));
        output.Write(Signature);
        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header, size.X);
        BinaryPrimitives.WriteUInt32BigEndian(header[4..], size.Y);
        header[8] = 8;
        header[9] = RgbaColourType;
        WriteChunk(output, "IHDR", header);
        int stride = size.X * PixelBytes;
        var raw = new byte[size.Y * (stride + 1)];
        for (int row = 0; row < size.Y; row++)
        {
            int line = row * (stride + 1);
            for (int column = 0; column < size.X; column++)
            {
                Rgba32 pixel = pixels[row * size.X + column];
                int at = line + 1 + column * PixelBytes;
                raw[at] = pixel.R;
                raw[at + 1] = pixel.G;
                raw[at + 2] = pixel.B;
                raw[at + 3] = pixel.A;
            }
        }
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(raw);
        WriteChunk(output, "IDAT", compressed.ToArray());
        WriteChunk(output, "IEND", []);
    }

    /// <summary>Reads an RGBA PNG, checking every chunk's CRC and the image's form.</summary>
    /// <param name="input">The stream read from its current position; the caller keeps ownership.</param>
    /// <param name="size">The image's width and height.</param>
    /// <returns>The pixels, row by row from the top.</returns>
    /// <exception cref="InvalidDataException">The data is not a well-formed 8-bit RGBA, non-interlaced PNG.</exception>
    public static Rgba32[] Read(Stream input, out Extent16<ImagePixels> size)
    {
        Ensure.NotNull(input);
        Span<byte> signature = stackalloc byte[8];
        input.ReadExactly(signature);
        if (!signature.SequenceEqual(Signature))
            throw new InvalidDataException("Not a PNG image.");
        int width = 0, height = 0;
        using var data = new MemoryStream();
        Span<byte> prefix = stackalloc byte[8];
        Span<byte> checksum = stackalloc byte[4];
        while (true)
        {
            input.ReadExactly(prefix);
            uint length = BinaryPrimitives.ReadUInt32BigEndian(prefix);
            if (length > int.MaxValue)
                throw new InvalidDataException("A PNG chunk is too long.");
            string type = Encoding.ASCII.GetString(prefix[4..]);
            var payload = new byte[length];
            input.ReadExactly(payload);
            input.ReadExactly(checksum);
            if (Crc(prefix[4..], payload) != BinaryPrimitives.ReadUInt32BigEndian(checksum))
                throw new InvalidDataException($"PNG chunk {type} fails its CRC.");
            if (type == "IHDR")
            {
                if (payload.Length != 13 || payload[8] != 8 || payload[9] != RgbaColourType || payload[10] != 0 || payload[11] != 0 || payload[12] != 0)
                    throw new InvalidDataException("Artwork PNGs are 8-bit RGBA without interlacing.");
                uint w = BinaryPrimitives.ReadUInt32BigEndian(payload), h = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(4));
                if (w is 0 or > MaximumDimension || h is 0 or > MaximumDimension)
                    throw new InvalidDataException($"A {w}x{h} PNG is outside the supported size.");
                width = (int)w;
                height = (int)h;
            }
            else if (type == "IDAT")
            {
                data.Write(payload);
            }
            else if (type == "IEND")
            {
                break;
            }
        }
        if (width == 0)
            throw new InvalidDataException("The PNG has no header.");
        size = new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)height));
        int stride = width * PixelBytes;
        var raw = new byte[height * (stride + 1)];
        data.Position = 0;
        using (var zlib = new ZLibStream(data, CompressionMode.Decompress, leaveOpen: true))
            zlib.ReadExactly(raw);
        var pixels = new Rgba32[width * height];
        var previous = new byte[stride];
        var current = new byte[stride];
        for (int row = 0; row < height; row++)
        {
            int line = row * (stride + 1);
            raw.AsSpan(line + 1, stride).CopyTo(current);
            Unfilter(raw[line], current, previous);
            for (int column = 0; column < width; column++)
            {
                int at = column * PixelBytes;
                pixels[row * width + column] = new Rgba32(current[at], current[at + 1], current[at + 2], current[at + 3]);
            }
            (previous, current) = (current, previous);
        }
        return pixels;
    }

    /// <summary>Reverses a row's PNG filter in place against the reconstructed row above.</summary>
    private static void Unfilter(byte filter, Span<byte> row, ReadOnlySpan<byte> above)
    {
        for (int index = 0; index < row.Length; index++)
        {
            int left = index >= PixelBytes ? row[index - PixelBytes] : 0;
            int up = above[index];
            int upLeft = index >= PixelBytes ? above[index - PixelBytes] : 0;
            row[index] = unchecked((byte)(row[index] + filter switch
            {
                0 => 0,
                1 => left,
                2 => up,
                3 => (left + up) / 2,
                4 => Paeth(left, up, upLeft),
                _ => throw new InvalidDataException($"Unknown PNG filter {filter}."),
            }));
        }
    }

    /// <summary>The PNG Paeth predictor.</summary>
    private static int Paeth(int left, int up, int upLeft)
    {
        int estimate = left + up - upLeft;
        int toLeft = Math.Abs(estimate - left), toUp = Math.Abs(estimate - up), toUpLeft = Math.Abs(estimate - upLeft);
        return toLeft <= toUp && toLeft <= toUpLeft ? left : toUp <= toUpLeft ? up : upLeft;
    }

    /// <summary>Writes one chunk with its length and CRC.</summary>
    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> number = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(number, (uint)data.Length);
        output.Write(number);
        byte[] name = Encoding.ASCII.GetBytes(type);
        output.Write(name);
        output.Write(data);
        BinaryPrimitives.WriteUInt32BigEndian(number, Crc(name, data));
        output.Write(number);
    }

    /// <summary>The PNG CRC-32 of a chunk's type and data.</summary>
    private static uint Crc(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        uint crc = uint.MaxValue;
        foreach (byte value in type) crc = Step(crc, value);
        foreach (byte value in data) crc = Step(crc, value);
        return ~crc;

        static uint Step(uint crc, byte value)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ ReflectedPolynomial : crc >> 1;
            return crc;
        }
    }

    /// <summary>The CRC-32 polynomial PNG uses, bit-reversed.</summary>
    private const uint ReflectedPolynomial = 0xedb88320;
}
