using System.Buffers.Binary;
using System.IO.Compression;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="PngWriter"/>; never linked by player hosts.</summary>
internal static class PngWriterTooling
{
    /// <summary>Eight-byte PNG header written before any image chunks.</summary>
    internal static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    /// <summary>Expands palette indexes to RGBA and writes them as a PNG.</summary>
    public static void WriteIndexedAsRgba<TSpace>(
        string path,
        Extent16<TSpace> size,
        ReadOnlySpan<byte> indexes,
        IReadOnlyList<RgbaPixel> palette,
        int scale = 1)
        where TSpace : ISpatialSpace
    {
        Ensure.LengthEqual(indexes, size.Area);
        if (palette.Count == 0)
            throw new ArgumentException("Palette is empty.", nameof(palette));

        var pixels = new RgbaPixel[indexes.Length];
        for (int i = 0; i < indexes.Length; i++)
            pixels[i] = palette[indexes[i] % palette.Count];
        WriteRgba(path, size, pixels, scale);
    }
    /// <summary>Writes an RGBA buffer, optionally nearest-neighbor scaled by an integer.</summary>
    public static void WriteRgba<TSpace>(
        string path,
        Extent16<TSpace> size,
        ReadOnlySpan<RgbaPixel> pixels,
        int scale = 1)
        where TSpace : ISpatialSpace
    {
        Ensure.LengthEqual(pixels, size.Area);
        ArgumentOutOfRangeException.ThrowIfLessThan(scale, 1);

        int width = size.X;
        int outputWidth = width * scale;
        int outputHeight = size.Y * scale;
        int scanlineBytes = checked(outputHeight * (1 + outputWidth * 4));
        // Stored DEFLATE blocks add five bytes per 64 KiB; zlib adds six. Leave far more than
        // that, so incompressible art still fits and the fixed stream never needs to grow.
        int compressedCapacity = checked(scanlineBytes + scanlineBytes / 8 + 1024);
        byte[] raw = System.Buffers.ArrayPool<byte>.Shared.Rent(scanlineBytes);
        byte[] deflated = System.Buffers.ArrayPool<byte>.Shared.Rent(compressedCapacity);
        try
        {
            int offset = 0;
            for (int y = 0; y < outputHeight; y++)
            {
                // Each PNG scanline starts with its filter method. "None" is slightly larger
                // than adaptive filtering but transparent and trivial to validate in a debugger.
                raw[offset++] = 0;
                int sourceY = y / scale;
                for (int x = 0; x < outputWidth; x++)
                {
                    RgbaPixel color = pixels[sourceY * width + x / scale];
                    raw[offset++] = color.R;
                    raw[offset++] = color.G;
                    raw[offset++] = color.B;
                    raw[offset++] = color.A;
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ??
                throw new ArgumentException($"{path} has no containing directory.", nameof(path)));
            using var file = File.Create(path);
            file.Write(Signature);
            Span<byte> header = stackalloc byte[13];
            BinaryPrimitives.WriteUInt32BigEndian(header, (uint)outputWidth);
            BinaryPrimitives.WriteUInt32BigEndian(header[4..], (uint)outputHeight);
            header[8] = 8;
            header[9] = 6; // PNG color type 6 is true-color RGBA.
            PngWriter.WriteChunk(file, "IHDR", header);

            // PNG's IDAT payload is a zlib stream, not a raw DEFLATE stream.
            using var compressed = new MemoryStream(deflated, 0, compressedCapacity, writable: true);
            using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
                zlib.Write(raw.AsSpan(0, scanlineBytes));
            PngWriter.WriteChunk(file, "IDAT", deflated.AsSpan(0, checked((int)compressed.Position)));
            PngWriter.WriteChunk(file, "IEND", []);
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(raw);
            System.Buffers.ArrayPool<byte>.Shared.Return(deflated);
        }
    }
}
