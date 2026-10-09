using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>Verifies legacy room-FX sheet dimensions, stock-tail inheritance, input-position preservation, and rewind requirements.</summary>
    private static void VerifyRoomFxLegacySheetSelection()
    {
        // Pre-conversion widths at1fefba07, independently fixed compatibility contract.
        // Uniform pixel1 encodes FF/00 per row; pixel2 encodes 00/FF.
        using var stockPng = Sheet(1808, 1);
        var stock = RoomFxAnimatedTileAtlas.Load(stockPng);
        foreach (int width in new[] { 1544, 776, 712, 1616, 1808 })
        {
            using var input = Sheet(width, 2);
            var loaded = RoomFxAnimatedTileAtlas.Load(input, stock);
            var transfer = (byte[])typeof(RoomFxAnimatedTileAtlas).GetField("transfer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(loaded)!;
            AssertEqual(3616, transfer.Length, "Legacy import produces full original sheet geometry");
            for (int index = 0; index < transfer.Length; index++)
            {
                byte expected = (byte)((index < width * 2 ? (index & 1) == 1 : (index & 1) == 0) ? 255 : 0);
                AssertEqual(expected, transfer[index], "Edited legacy prefix and inherited stock tail");
            }
            if (width == 1808) continue;
            input.Position = 7;
            AssertThrows<InvalidDataException>(() => RoomFxAnimatedTileAtlas.Load(input), "Legacy dimensions need stock tail");
            using var forwardOnly = new FxAtlasForwardOnlyStream(input.ToArray()[7..]);
            AssertThrows<InvalidDataException>(() => RoomFxAnimatedTileAtlas.Load(forwardOnly, stock), "Legacy fallback requires rewind");
        }
        foreach (int width in new[] { 8, 704, 720, 768, 784, 1536, 1552, 1608, 1624 })
        {
            using var invalid = Sheet(width, 2);
            AssertThrows<InvalidDataException>(() => RoomFxAnimatedTileAtlas.Load(invalid, stock), "Unsupported neighboring geometry rejects");
        }
        using var current = Sheet(1808, 2);
        using var nonSeekable = new FxAtlasForwardOnlyStream(current.ToArray()[7..]);
        _ = RoomFxAnimatedTileAtlas.Load(nonSeekable, stock);
        AssertThrows<ArgumentNullException>(() => RoomFxAnimatedTileAtlas.Load(null!), "Null PNG still rejects");

        static MemoryStream Sheet(int width, byte pixel)
        {
            var png = new MemoryStream();
            png.Write(new byte[7]); // Loading must rewind to the caller's initial offset.
            IndexedPng.Write(png, width, 8, Enumerable.Repeat(pixel, width * 8).ToArray(),
                new[] { new Rgba32(0, 0, 0, 255), new Rgba32(85, 85, 85, 255),
                    new Rgba32(170, 170, 170, 255), new Rgba32(255, 255, 255, 255) });
            png.Position = 7;
            return png;
        }
    }

    /// <summary>Memory-backed test stream that exposes data sequentially while refusing all seek operations.</summary>
    /// <param name="bytes">PNG bytes available to forward-only readers.</param>
    private sealed class FxAtlasForwardOnlyStream(byte[] bytes) : MemoryStream(bytes)
    {
        /// <summary>Always reports false so consumers can detect that this stream cannot be rewound.</summary>
        public override bool CanSeek => false;

        /// <summary>Seeking by position is unsupported for this forward-only test stream.</summary>
        /// <exception cref="NotSupportedException">The position is read or changed.</exception>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <summary>Rejects attempts to reposition the stream.</summary>
        /// <param name="offset">Relative or absolute offset requested by the caller.</param>
        /// <param name="loc">Origin from which the offset would be applied.</param>
        /// <returns>This method never returns.</returns>
        /// <exception cref="NotSupportedException">Seeking is not supported.</exception>
        public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();
    }
}
