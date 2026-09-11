using System.Buffers.Binary;
using System.IO.Compression;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyIndexedPng()
    {
        var palette = Enumerable.Range(0, 16).Select(i => new Rgba32((byte)(i * 17), 31, 71, (byte)(i == 0 ? 0 : 255))).ToArray();
        byte[] indexes = Enumerable.Range(0, 33).Select(i => (byte)(i % 16)).ToArray();
        using var encoded = new MemoryStream();
        IndexedPng.Write(encoded, 11, 3, indexes, palette);
        byte[] png = encoded.ToArray();
        AssertEqual((byte)3, png[25], "PNG writer emits indexed color type, not expanded RGBA");
        AssertTrue(png.AsSpan(png.Length - 4).SequenceEqual(new byte[] { 0xae, 0x42, 0x60, 0x82 }), "PNG IEND CRC matches published wire value");
        var decoded = IndexedPng.Read(new MemoryStream(png), 11, 3);
        AssertTrue(indexes.AsSpan().SequenceEqual(decoded.Pixels), "indexed PNG retains original indexes");
        AssertTrue(palette.AsSpan().SequenceEqual(decoded.Palette), "indexed PNG retains RGB and transparent palette entry");
        AssertThrows<InvalidDataException>(() => IndexedPng.Read(new MemoryStream(png), 12, 3), "PNG wrong dimensions rejected");
        AssertThrows<InvalidDataException>(() => IndexedPng.Read(new MemoryStream(png[..^1]), 11, 3), "PNG truncation rejected");
        byte[] corrupt = png.ToArray(); corrupt[29] ^= 1;
        AssertThrows<InvalidDataException>(() => IndexedPng.Read(new MemoryStream(corrupt), 11, 3), "PNG bad CRC rejected");
        AssertThrows<InvalidDataException>(() => IndexedPng.Read(new MemoryStream([.. png, 0]), 11, 3), "PNG trailing bytes rejected");
        AssertThrows<ArgumentException>(() => IndexedPng.Write(new MemoryStream(), 1, 1, [16], palette), "writer rejects missing palette index rather than applying modulo");

        // Independent hand-calculated scanlines, not a decoder/encoder inverse pair.
        // Decoded rows are 1,2,3 through 13,14,15 using None/Sub/Up/Average/Paeth.
        byte[] filters = [0,1,2,3, 1,4,1,1, 2,3,3,3, 3,7,2,2, 4,3,1,1];
        byte[] filtered = PngFixture(3, 5, 8, filters);
        AssertTrue(IndexedPng.Read(new MemoryStream(filtered), 3, 5).Pixels.AsSpan()
            .SequenceEqual(Enumerable.Range(1, 15).Select(i => (byte)i).ToArray()), "all PNG filters recover independently specified rows");
        foreach (int depth in new[] { 1, 2, 4, 8 })
        {
            const int width = 11;
            int stride = (width * depth + 7) / 8;
            byte[] row = new byte[stride + 1], expected = new byte[width];
            for (int x = 0; x < width; x++)
            {
                expected[x] = (byte)(x % Math.Min(16, 1 << depth));
                row[1 + x * depth / 8] |= (byte)(expected[x] << (8 - depth - x * depth % 8));
            }
            byte[] fixture = PngFixture(width, 1, depth, row);
            AssertTrue(expected.AsSpan().SequenceEqual(IndexedPng.Read(new MemoryStream(fixture), width, 1).Pixels),
                $"indexed PNG depth {depth} preserves odd-width packed pixels");
        }
        AssertInvalid(PngFixture(1, 1, 8, [5, 0]), "unsupported row filter");
        AssertInvalid(PngFixture(1, 1, 8, [0, 16]), "index beyond PLTE");
        AssertInvalid(PngFixture(1, 1, 8, [0]), "short inflated row");
        AssertInvalid(PngFixture(1, 1, 8, [0, 0, 0]), "excess inflated data");
        AssertInvalid(PngFixture(1, 1, 8, [0, 0], colorType: 6), "RGBA is not an indexed atlas");
        AssertInvalid(PngFixture(1, 1, 8, [0, 0], interlace: 1), "unsupported interlace");
        AssertInvalid(PngFixture(1, 1, 8, [0, 0], extraChunk: "acTL"), "animation not silently ignored");
        AssertInvalid(PngFixture(1, 1, 8, [0, 0], extraChunk: "ABCD"), "unknown critical chunk");
        AssertEqual((byte)0, IndexedPng.Read(new MemoryStream(PngFixture(1, 1, 8, [0, 0], extraChunk: "tEXt")), 1, 1).Pixels[0],
            "ancillary metadata leaves indexes unchanged");
        Console.WriteLine("Indexed PNG: index/palette parity, packed depths, five independent filter rows and malformed-resource failures pass.");

        void AssertInvalid(byte[] bytes, string context) => AssertThrows<InvalidDataException>(
            () => IndexedPng.Read(new MemoryStream(bytes), 1, 1), context);
    }

    private static byte[] PngFixture(int width, int height, int depth, byte[] filteredRows,
        byte colorType = 3, byte interlace = 0, string? extraChunk = null)
    {
        using var output = new MemoryStream();
        output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        byte[] header = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header, (uint)width);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), (uint)height);
        header[8] = (byte)depth; header[9] = colorType; header[12] = interlace;
        PngWriter.WriteChunk(output, "IHDR", header);
        PngWriter.WriteChunk(output, "PLTE", new byte[Math.Min(16, 1 << depth) * 3]);
        if (extraChunk is not null) PngWriter.WriteChunk(output, extraChunk, []);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, true)) zlib.Write(filteredRows);
        // Multiple consecutive IDAT chunks must decode as one zlib stream.
        byte[] bytes = compressed.ToArray();
        PngWriter.WriteChunk(output, "IDAT", bytes.AsSpan(0, bytes.Length / 2));
        PngWriter.WriteChunk(output, "IDAT", bytes.AsSpan(bytes.Length / 2));
        PngWriter.WriteChunk(output, "IEND", []);
        return output.ToArray();
    }
}
