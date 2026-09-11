using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Minimal RGBA PNG encoder used to keep the reverse-engineering tools dependency-free.
/// Supporting one predictable format also makes generated assets deterministic.
/// </summary>
public static class PngWriter
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    /// <summary>Expands palette indexes to RGBA and writes them as a PNG.</summary>
    public static void WriteIndexedAsRgba(
        string path,
        int width,
        int height,
        ReadOnlySpan<byte> indexes,
        IReadOnlyList<Rgba32> palette,
        int scale = 1)
    {
        if (indexes.Length != width * height)
            throw new ArgumentException("Pixel count does not match dimensions.", nameof(indexes));
        if (palette.Count == 0)
            throw new ArgumentException("Palette is empty.", nameof(palette));

        var pixels = new Rgba32[indexes.Length];
        for (int i = 0; i < indexes.Length; i++)
            pixels[i] = palette[indexes[i] % palette.Count];
        WriteRgba(path, width, height, pixels, scale);
    }

    /// <summary>Writes an RGBA buffer, optionally nearest-neighbor scaled by an integer.</summary>
    public static void WriteRgba(
        string path,
        int width,
        int height,
        ReadOnlySpan<Rgba32> pixels,
        int scale = 1)
    {
        if (pixels.Length != width * height)
            throw new ArgumentException("Pixel count does not match dimensions.", nameof(pixels));
        ArgumentOutOfRangeException.ThrowIfLessThan(scale, 1);

        int outputWidth = width * scale;
        int outputHeight = height * scale;
        using var scanlines = new MemoryStream();
        for (int y = 0; y < outputHeight; y++)
        {
            // Each PNG scanline starts with its filter method. "None" is slightly larger
            // than adaptive filtering but transparent and trivial to validate in a debugger.
            scanlines.WriteByte(0);
            int sourceY = y / scale;
            for (int x = 0; x < outputWidth; x++)
            {
                Rgba32 color = pixels[sourceY * width + x / scale];
                scanlines.WriteByte(color.R);
                scanlines.WriteByte(color.G);
                scanlines.WriteByte(color.B);
                scanlines.WriteByte(color.A);
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var file = File.Create(path);
        file.Write(Signature);
        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header, (uint)outputWidth);
        BinaryPrimitives.WriteUInt32BigEndian(header[4..], (uint)outputHeight);
        header[8] = 8;
        header[9] = 6; // PNG color type 6 is true-color RGBA.
        WriteChunk(file, "IHDR", header);

        // PNG's IDAT payload is a zlib stream, not a raw DEFLATE stream.
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
            zlib.Write(scanlines.GetBuffer().AsSpan(0, checked((int)scanlines.Length)));
        WriteChunk(file, "IDAT", compressed.GetBuffer().AsSpan(0, checked((int)compressed.Length)));
        WriteChunk(file, "IEND", []);
    }

    /// <summary>Writes a length/type/data/CRC PNG chunk.</summary>
    internal static void WriteChunk(Stream output, string name, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
        output.Write(length);
        byte[] type = Encoding.ASCII.GetBytes(name);
        output.Write(type);
        output.Write(data);

        // The CRC covers the four-byte type and payload, but not the length field.
        uint crc = 0xffffffff;
        foreach (byte value in type) crc = UpdateCrc(crc, value);
        foreach (byte value in data) crc = UpdateCrc(crc, value);
        Span<byte> checksum = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksum, ~crc);
        output.Write(checksum);
    }

    internal static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (int bit = 0; bit < 8; bit++)
            crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xedb88320 : crc >> 1;
        return crc;
    }
}
