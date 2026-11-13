using SuperMetroid.Core.Assets;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Reads and writes the installation's pre-coloured artwork: 8-bit RGBA PNG images, non-interlaced. A pixel with zero
/// alpha is transparent. Anything else (indexed or greyscale colour, other bit depths, interlacing) is rejected, so an
/// edited image is saved in the same form as the stock one. The codec owns the file's byte-wide filter bytes, channel
/// samples and chunk checksums.
/// </summary>
[EncodingOwner("RGBA PNG file")]
public static class RgbaPng
{
    /// <summary>The eight-byte PNG signature.</summary>
    private static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];

    /// <summary>The colour type of 8-bit-per-channel RGBA.</summary>
    private const byte RgbaColourType = 6;

    /// <summary>Bytes per RGBA pixel.</summary>
    private const int PixelBytes = 4;

    /// <summary>The largest dimension accepted, which bounds a malformed header's allocation; artwork is composed within it.</summary>
    internal const int MaximumDimension = 8192;


    /// <summary>
    /// Encodes an RGBA image as a PNG file's bytes: the header chunk, every row unfiltered and compressed as one zlib
    /// stream at the optimal level in one data chunk, and the end chunk. The rows and their compressed stream are laid out
    /// in <paramref name="buffers"/>, which the caller reuses from one image to the next (#1490), so encoding allocates
    /// only the finished file once the buffers have grown to the image's size.
    /// </summary>
    /// <param name="size">The image's width and height; both are positive.</param>
    /// <param name="pixels">Width times height pixels, row by row from the top.</param>
    /// <param name="buffers">The working buffers of the thread encoding the image.</param>
    /// <returns>The whole PNG file.</returns>
    /// <exception cref="ArgumentException">A dimension is zero or the pixel count does not match.</exception>
    internal static byte[] Encode(Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels, RgbaPngBuffers buffers)
    {
        Ensure.NotNull(buffers);
        if (size.X == 0 || size.Y == 0 || pixels.Length != size.X * size.Y)
            throw new ArgumentException($"A {size.X}x{size.Y} image needs {size.X * size.Y} pixels, not {pixels.Length}.", nameof(pixels));
        int stride = size.X * PixelBytes;
        Span<byte> raw = buffers.Scanlines(size.Y * (stride + 1));
        for (int row = 0; row < size.Y; row++)
        {
            Span<byte> line = raw.Slice(row * (stride + 1), stride + 1);
            // The reused buffer still holds the previous image's rows, so each row's filter byte is written too.
            line[0] = NoFilter;
            // A pixel's four channels are laid out in the order the file stores them.
            MemoryMarshal.AsBytes(pixels.Slice(row * size.X, size.X)).CopyTo(line[1..]);
        }
        MemoryStream compressed = buffers.EmptyCompressed();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(raw);
        ReadOnlySpan<byte> data = compressed.GetBuffer().AsSpan(0, checked((int)compressed.Length));
        var file = new byte[Signature.Length + ChunkBytes(HeaderLength) + ChunkBytes(data.Length) + ChunkBytes(0)];
        Signature.CopyTo(file);
        Span<byte> header = stackalloc byte[HeaderLength];
        BinaryPrimitives.WriteUInt32BigEndian(header, size.X);
        BinaryPrimitives.WriteUInt32BigEndian(header[4..], size.Y);
        header[8] = 8;
        header[9] = RgbaColourType;
        Span<byte> rest = WriteChunk(file.AsSpan(Signature.Length), "IHDR"u8, header);
        rest = WriteChunk(rest, "IDAT"u8, data);
        WriteChunk(rest, "IEND"u8, []);
        return file;
    }

    /// <summary>The filter type of a row stored as its raw bytes.</summary>
    private const byte NoFilter = 0;

    /// <summary>The bytes of a chunk's type name.</summary>
    private const int ChunkTypeLength = 4;

    /// <summary>The bytes a chunk of <paramref name="dataLength"/> data bytes takes with its length, type and CRC.</summary>
    private static int ChunkBytes(int dataLength) => sizeof(uint) + ChunkTypeLength + dataLength + sizeof(uint);

    /// <summary>
    /// Reads only an RGBA PNG's size from its header chunk, which follows the signature, checking its CRC and form as
    /// <see cref="Read"/> does.
    /// </summary>
    /// <param name="input">The stream read from its current position; the caller keeps ownership.</param>
    /// <exception cref="InvalidDataException">The data does not begin with a well-formed 8-bit RGBA, non-interlaced PNG header.</exception>
    public static Extent16<ImagePixels> ReadSize(Stream input)
    {
        Ensure.NotNull(input);
        Span<byte> signature = stackalloc byte[8];
        input.ReadExactly(signature);
        if (!signature.SequenceEqual(Signature))
            throw new InvalidDataException("Not a PNG image.");
        Span<byte> prefix = stackalloc byte[8];
        input.ReadExactly(prefix);
        if (BinaryPrimitives.ReadUInt32BigEndian(prefix) != HeaderLength || Encoding.ASCII.GetString(prefix[4..]) != "IHDR")
            throw new InvalidDataException("The PNG has no header.");
        var payload = new byte[HeaderLength];
        input.ReadExactly(payload);
        Span<byte> checksum = stackalloc byte[4];
        input.ReadExactly(checksum);
        if (Crc(prefix[4..], payload) != BinaryPrimitives.ReadUInt32BigEndian(checksum))
            throw new InvalidDataException("PNG chunk IHDR fails its CRC.");
        return Header(payload);
    }

    /// <summary>The bytes of a PNG header chunk.</summary>
    private const int HeaderLength = 13;

    /// <summary>The width and height a header chunk names, which must describe an 8-bit RGBA, non-interlaced image.</summary>
    /// <exception cref="InvalidDataException">The header names another form or a size outside the supported one.</exception>
    private static Extent16<ImagePixels> Header(byte[] payload)
    {
        if (payload.Length != HeaderLength || payload[8] != 8 || payload[9] != RgbaColourType || payload[10] != 0 || payload[11] != 0 || payload[12] != 0)
            throw new InvalidDataException("Artwork PNGs are 8-bit RGBA without interlacing.");
        uint w = BinaryPrimitives.ReadUInt32BigEndian(payload), h = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(4));
        if (w is 0 or > MaximumDimension || h is 0 or > MaximumDimension)
            throw new InvalidDataException($"A {w}x{h} PNG is outside the supported size.");
        return new Extent16<ImagePixels>(checked((ushort)w), checked((ushort)h));
    }

    /// <summary>Reads an RGBA PNG, checking every chunk's CRC and the image's form.</summary>
    /// <param name="input">The stream read from its current position; the caller keeps ownership.</param>
    /// <param name="size">The image's width and height.</param>
    /// <returns>The pixels, row by row from the top.</returns>
    /// <exception cref="InvalidDataException">The data is not a well-formed 8-bit RGBA, non-interlaced PNG.</exception>
    public static RgbaPixel[] Read(Stream input, out Extent16<ImagePixels> size)
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
                Extent16<ImagePixels> header = Header(payload);
                width = header.X;
                height = header.Y;
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
        data.Position = 0;
        // Each row is inflated and unfiltered as it is read, so the decoded image is the only image-sized allocation.
        using var zlib = new ZLibStream(data, CompressionMode.Decompress, leaveOpen: true);
        var pixels = new RgbaPixel[width * height];
        var previous = new byte[stride];
        var current = new byte[stride];
        Span<byte> filter = stackalloc byte[1];
        for (int row = 0; row < height; row++)
        {
            zlib.ReadExactly(filter);
            zlib.ReadExactly(current);
            Unfilter(filter[0], current, previous);
            // A pixel's four channels are laid out in the order the file stores them, as Encode writes them.
            MemoryMarshal.Cast<byte, RgbaPixel>(current).CopyTo(pixels.AsSpan(row * width, width));
            byte[] filtered = previous;
            previous = current;
            current = filtered;
        }
        return pixels;
    }

    /// <summary>
    /// Reverses a row's PNG filter in place against the reconstructed row above: none, left, up, average or Paeth. A byte
    /// left of the row's first pixel, or above-left of it, is zero.
    /// </summary>
    /// <exception cref="InvalidDataException">The filter is none of the five.</exception>
    private static void Unfilter(byte filter, Span<byte> row, ReadOnlySpan<byte> above)
    {
        switch (filter)
        {
            case 0:
                return;
            case 1:
                for (int index = PixelBytes; index < row.Length; index++)
                    row[index] = unchecked((byte)(row[index] + row[index - PixelBytes]));
                return;
            case 2:
                for (int index = 0; index < row.Length; index++)
                    row[index] = unchecked((byte)(row[index] + above[index]));
                return;
            case 3:
                for (int index = 0; index < row.Length; index++)
                {
                    int left = index >= PixelBytes ? row[index - PixelBytes] : 0;
                    row[index] = unchecked((byte)(row[index] + (left + above[index]) / 2));
                }
                return;
            case 4:
                for (int index = 0; index < row.Length; index++)
                {
                    int left = index >= PixelBytes ? row[index - PixelBytes] : 0;
                    int upLeft = index >= PixelBytes ? above[index - PixelBytes] : 0;
                    row[index] = unchecked((byte)(row[index] + Paeth(left, above[index], upLeft)));
                }
                return;
            default:
                throw new InvalidDataException($"Unknown PNG filter {filter}.");
        }
    }

    /// <summary>The PNG Paeth predictor.</summary>
    private static int Paeth(int left, int up, int upLeft)
    {
        int estimate = left + up - upLeft;
        int toLeft = Math.Abs(estimate - left), toUp = Math.Abs(estimate - up), toUpLeft = Math.Abs(estimate - upLeft);
        return toLeft <= toUp && toLeft <= toUpLeft ? left : toUp <= toUpLeft ? up : upLeft;
    }

    /// <summary>Writes one chunk with its length and CRC at the start of <paramref name="destination"/>.</summary>
    /// <returns>The rest of <paramref name="destination"/>, after the chunk.</returns>
    private static Span<byte> WriteChunk(Span<byte> destination, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        BinaryPrimitives.WriteUInt32BigEndian(destination, (uint)data.Length);
        type.CopyTo(destination[sizeof(uint)..]);
        data.CopyTo(destination[(sizeof(uint) + type.Length)..]);
        BinaryPrimitives.WriteUInt32BigEndian(destination[(sizeof(uint) + type.Length + data.Length)..], Crc(type, data));
        return destination[ChunkBytes(data.Length)..];
    }

    /// <summary>The PNG CRC-32 of a chunk's type and data.</summary>
    private static uint Crc(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        uint crc = uint.MaxValue;
        foreach (byte value in type) crc = Step(crc, value);
        foreach (byte value in data) crc = Step(crc, value);
        return ~crc;

        static uint Step(uint crc, byte value) => CrcOfByte[unchecked((byte)(crc ^ value))] ^ (crc >> 8);
    }

    /// <summary>
    /// The CRC-32 remainder of each byte value, shifted out bit by bit through <see cref="ReflectedPolynomial"/>, so a
    /// CRC advances a whole byte at a time.
    /// </summary>
    private static readonly uint[] CrcOfByte = CrcRemainders();

    /// <summary>Computes <see cref="CrcOfByte"/>.</summary>
    private static uint[] CrcRemainders()
    {
        var remainders = new uint[byte.MaxValue + 1];
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            uint crc = (uint)value;
            for (int bit = 0; bit < 8; bit++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ ReflectedPolynomial : crc >> 1;
            remainders[value] = crc;
        }
        return remainders;
    }

    /// <summary>The CRC-32 polynomial PNG uses, bit-reversed.</summary>
    private const uint ReflectedPolynomial = 0xedb88320; // cartridge-encoding: allow(WireFormat) - PNG's reflected CRC-32 polynomial
}
