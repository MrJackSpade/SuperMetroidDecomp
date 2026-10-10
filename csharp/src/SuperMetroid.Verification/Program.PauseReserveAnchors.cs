using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Validates six ROM-derived reserve-tank anchors and exercises editable horizontal and vertical coordinates.</summary>
    /// <param name="rom">Retail address space supplying the native reserve-tank coordinate tables and sprite data.</param>
    private static void VerifyPauseReserveAnchors(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseReserveTankExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<PauseReserveTankDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var stock = PauseReserveTankPresentation.Load(new MemoryStream(bytes));
        AssertEqual(0, stock.StoredAnchorComponentCount, "stock reserve coordinates are calculated");
        Suite(nameof(VerifyPauseReserveAnchorField), () => VerifyPauseReserveAnchorField(rom, document, stock, true));
        Suite(nameof(VerifyPauseReserveAnchorField), () => VerifyPauseReserveAnchorField(rom, document, stock, false));
        foreach (int invalid in new[] { int.MinValue, -1, 6, 256, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => stock.Anchor(invalid), "reserve anchor array boundary preserved");
    }

    /// <summary>Checks one anchor coordinate against ROM data, verifies edits preserve other fields, and compares the resulting OAM with a native draw.</summary>
    /// <param name="rom">Retail address space used to resolve the native anchor and spritemap.</param>
    /// <param name="document">Extracted editable anchor document whose selected component is varied.</param>
    /// <param name="stock">Loaded presentation used to inspect default anchors and render the edited sprite.</param>
    /// <param name="horizontal">Selects X when true and Y when false, including that axis's valid coordinate bounds.</param>
    private static void VerifyPauseReserveAnchorField(ISnesAddressSpace rom, PauseReserveTankDocument document,
        PauseReserveTankPresentation stock, bool horizontal)
    {
        for (int index = 0; index < 6; index++)
        {
            int expected = horizontal ? ReadVerificationWord(rom, 0x82c1d6 + index * 2) : ReadVerificationWord(rom, 0x82c1e2) - 1;
            var point = stock.Anchor(index);
            var basis = PauseReserveTankDefinitions.StockAnchor(index);
            AssertEqual(expected, horizontal ? point.X : point.Y, "original installed reserve coordinate");
            AssertEqual(expected, horizontal ? basis.X : basis.Y, "original reserve geometry");
            foreach (int value in new[] { 0, horizontal ? 255 : 223 })
            {
                var anchors = (MapLabelPoint[])document.Anchors.Clone();
                anchors[index] = horizontal ? new(value, anchors[index].Y) : new(anchors[index].X, value);
                using var stream = new MemoryStream(); PauseReserveTankPresentation.Write(stream, document with { Anchors = anchors }); stream.Position = 0;
                var edited = PauseReserveTankPresentation.Load(stream);
                AssertEqual(1, edited.StoredAnchorComponentCount, "independent reserve coordinate edit");
                for (int other = 0; other < 6; other++) AssertEqual(anchors[other], edited.Anchor(other), "all other reserve fields preserved");
                int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + 0x1b * 2);
                var native = new OamBuffer(); var actual = new OamBuffer(); native.BeginFrame(); actual.BeginFrame();
                DrawImportedSpritemap(rom, native, pointer, (ushort)anchors[index].X, (ushort)anchors[index].Y, 0x600);
                edited.Draw(actual, 0x1b, index);
                native.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable),
                    "edited reserve coordinates reach actual sprite output");
            }
        }
        foreach (int invalid in new[] { -1, horizontal ? 256 : 224 })
        {
            var anchors = (MapLabelPoint[])document.Anchors.Clone();
            anchors[0] = horizontal ? new(invalid, anchors[0].Y) : new(anchors[0].X, invalid);
            AssertThrows<InvalidDataException>(() => PauseReserveTankPresentation.Write(new MemoryStream(), document with { Anchors = anchors }),
                "invalid authored reserve coordinate");
        }
    }
}
