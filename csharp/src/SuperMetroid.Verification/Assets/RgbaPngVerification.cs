using System.Buffers.Binary;
using System.IO.Compression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Assets;

/// <summary>Checks the RGBA PNG decoder's row filters, pixel order and chunk checksums (#1680).</summary>
internal static class RgbaPngVerification
{
    /// <summary>Bytes per RGBA pixel in the file.</summary>
    private const int PixelBytes = 4;

    /// <summary>
    /// Decodes an image whose rows a forward filter written from the PNG specification filters with each of the five PNG
    /// filters, both on the first row and below a filled row, split over two data chunks whose checksums the asset
    /// writer computes: every pixel's four channels come back in order. An unknown filter and a corrupted checksum are
    /// rejected, and the codec's own unfiltered encoding round-trips.
    /// </summary>
    internal static void VerifyRgbaPngFilters()
    {
        const int width = 3;
        const int filters = 5;
        var size = new Extent16<ImagePixels>(width, filters * 2);
        var random = new Random(1680);
        var pixels = new RgbaPixel[size.X * size.Y];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
            pixels[pixel] = new RgbaPixel((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

        // Rows 0-4 use filters 0-4 over the row above (none on the first row); rows 5-9 repeat them below filled rows.
        byte[] png = Png(size, Filtered(size, pixels, row => (byte)(row % filters)));
        RgbaPixel[] decoded = RgbaPng.Read(new MemoryStream(png), out Extent16<ImagePixels> decodedSize);
        AssertEqual(size, decodedSize, "RGBA PNG size");
        for (int pixel = 0; pixel < pixels.Length; pixel++)
            AssertEqual(pixels[pixel], decoded[pixel], $"RGBA PNG pixel {pixel % width},{pixel / width} under filter {pixel / width % filters}");

        AssertThrows<InvalidDataException>(() => RgbaPng.Read(new MemoryStream(Png(size, Filtered(size, pixels, row => row == 3 ? (byte)filters : (byte)0))), out _),
            "an unknown row filter is rejected");
        byte[] corrupt = png.ToArray();
        // The signature, then the header chunk's length, type, thirteen bytes and checksum, then the first data chunk's
        // length and type: its first data byte.
        corrupt[8 + 8 + 13 + 4 + 8] ^= 1;
        AssertThrows<InvalidDataException>(() => RgbaPng.Read(new MemoryStream(corrupt), out _), "a corrupted data chunk fails its checksum");

        using var buffers = new RgbaPngBuffers();
        RgbaPixel[] roundTripped = RgbaPng.Read(new MemoryStream(RgbaPng.Encode(size, pixels, buffers)), out _);
        AssertTrue(pixels.AsSpan().SequenceEqual(roundTripped), "the codec's unfiltered encoding round-trips");
        Console.WriteLine("RGBA PNG: all five row filters, on the first row and below filled rows, decode every channel in order; unknown filters and bad checksums are rejected.");
    }

    /// <summary>The rows of <paramref name="pixels"/>, each led by the filter <paramref name="filterOf"/> gives it and filtered with it.</summary>
    private static byte[] Filtered(Extent16<ImagePixels> size, RgbaPixel[] pixels, Func<int, byte> filterOf)
    {
        int stride = size.X * PixelBytes;
        var raw = new byte[size.Y * stride];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            raw[pixel * PixelBytes] = pixels[pixel].R;
            raw[pixel * PixelBytes + 1] = pixels[pixel].G;
            raw[pixel * PixelBytes + 2] = pixels[pixel].B;
            raw[pixel * PixelBytes + 3] = pixels[pixel].A;
        }
        var filtered = new byte[size.Y * (stride + 1)];
        for (int row = 0; row < size.Y; row++)
        {
            byte filter = filterOf(row);
            filtered[row * (stride + 1)] = filter;
            for (int index = 0; index < stride; index++)
            {
                int at = row * stride + index;
                int left = index >= PixelBytes ? raw[at - PixelBytes] : 0;
                int up = row > 0 ? raw[at - stride] : 0;
                int upLeft = row > 0 && index >= PixelBytes ? raw[at - stride - PixelBytes] : 0;
                int predicted = filter switch
                {
                    0 => 0,
                    1 => left,
                    2 => up,
                    3 => (left + up) / 2,
                    4 => Paeth(left, up, upLeft),
                    // An unknown filter's row is stored as it is; the decoder must reject it.
                    _ => 0,
                };
                filtered[row * (stride + 1) + 1 + index] = unchecked((byte)(raw[at] - predicted));
            }
        }
        return filtered;
    }

    /// <summary>The PNG specification's Paeth predictor: whichever neighbour is nearest their sum less the corner.</summary>
    private static int Paeth(int left, int up, int upLeft)
    {
        int estimate = left + up - upLeft;
        int toLeft = Math.Abs(estimate - left);
        int toUp = Math.Abs(estimate - up);
        int toUpLeft = Math.Abs(estimate - upLeft);
        if (toLeft <= toUp && toLeft <= toUpLeft)
            return left;
        return toUp <= toUpLeft ? up : upLeft;
    }

    /// <summary>An 8-bit RGBA PNG of <paramref name="filteredRows"/>, compressed as one stream split over two data chunks.</summary>
    private static byte[] Png(Extent16<ImagePixels> size, byte[] filteredRows)
    {
        using var output = new MemoryStream();
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        var header = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header, size.X);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), size.Y);
        header[8] = 8;
        header[9] = 6;
        PngWriter.WriteChunk(output, "IHDR", header);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
            zlib.Write(filteredRows);
        byte[] data = compressed.ToArray();
        PngWriter.WriteChunk(output, "IDAT", data.AsSpan(0, data.Length / 2));
        PngWriter.WriteChunk(output, "IDAT", data.AsSpan(data.Length / 2));
        PngWriter.WriteChunk(output, "IEND", []);
        return output.ToArray();
    }
}
