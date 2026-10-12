using SuperMetroid.Core.Assets;

/// <summary>Checks legacy room-FX sheet widths against full stock artwork and stream requirements.</summary>
internal static class LookupFxLegacySheetsVerification
{
    /// <summary>Confirms five legacy widths inherit an unchanged stock tail, neighboring widths fail, and rewinding is required only for legacy fallback.</summary>
    internal static void VerifyRoomFxLegacySheetSelection()
    {
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
            IndexedPng.Write(png, new(checked((ushort)width), 8), Enumerable.Repeat(pixel, width * 8).ToArray(),
                new[] { new Rgba32(0, 0, 0, 255), new Rgba32(85, 85, 85, 255),
                    new Rgba32(170, 170, 170, 255), new Rgba32(255, 255, 255, 255) });
            png.Position = 7;
            return png;
        }
    }

    /// <summary>Fixture stream that exposes bytes but rejects positioning used by legacy-sheet fallback.</summary>
    internal sealed class FxAtlasForwardOnlyStream(byte[] bytes) : MemoryStream(bytes)
    {
        /// <summary>Forces the loader to treat this input as forward-only.</summary>
        public override bool CanSeek => false;
        /// <summary>Rejects both position inspection and reassignment to catch hidden rewinds.</summary>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>Rejects seeking while permitting sequential reads through the base stream.</summary>
        public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();
    }
}
