using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks reserve tank OAM anchors against cartridge coordinates and edited presentation.</summary>
internal static class PauseReserveAnchorsVerification
{
    /// <summary>Checks stock positions, independent axis edits, sprite output, and anchor bounds.</summary>
    internal static void VerifyPauseReserveAnchors(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseReserveTankExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveTankDocument>(bytes, ArtworkJson.Options);
        var stock = PauseReserveTankPresentation.Load(new MemoryStream(bytes));
        AssertEqual(0, stock.StoredAnchorComponentCount, "stock reserve coordinates are calculated");
        Suite(nameof(VerifyPauseReserveAnchorField), () => VerifyPauseReserveAnchorField(rom, document, stock, true));
        Suite(nameof(VerifyPauseReserveAnchorField), () => VerifyPauseReserveAnchorField(rom, document, stock, false));
        foreach (int invalid in new[] { int.MinValue, -1, 6, 256, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => stock.Anchor(invalid), "reserve anchor array boundary preserved");
    }

    /// <summary>Checks one anchor coordinate axis against native words and edited OAM placements.</summary>
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
                var anchors = (Position16<OamPixels>[])document.Anchors.Clone();
                anchors[index] = horizontal ? new(checked((ushort)value), anchors[index].Y) : new(anchors[index].X, checked((ushort)value));
                using var stream = new MemoryStream(); PauseReserveTankPresentation.Write(stream, document with { Anchors = anchors }); stream.Position = 0;
                var edited = PauseReserveTankPresentation.Load(stream);
                AssertEqual(1, edited.StoredAnchorComponentCount, "independent reserve coordinate edit");
                for (int other = 0; other < 6; other++) AssertEqual(anchors[other], edited.Anchor(other), "all other reserve fields preserved");
                int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + 0x1b * 2);
                var native = new OamBuffer(); var actual = new OamBuffer(); native.BeginFrame(); actual.BeginFrame();
                DrawImportedSpritemap(rom, native, pointer, new((ushort)anchors[index].X, (ushort)anchors[index].Y), 0x600);
                edited.Draw(actual, PauseReserveTankVisual.Full, index);
                native.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(native.ImageEquals(actual),
                    "edited reserve coordinates reach actual sprite output");
            }
        }
        foreach (int invalid in new[] { -1, horizontal ? 256 : 224 })
        {
            var anchors = (Position16<OamPixels>[])document.Anchors.Clone();
            anchors[0] = horizontal ? new(unchecked((ushort)invalid), anchors[0].Y) : new(anchors[0].X, unchecked((ushort)invalid));
            AssertThrows<InvalidDataException>(() => PauseReserveTankPresentation.Write(new MemoryStream(), document with { Anchors = anchors }),
                "invalid authored reserve coordinate");
        }
    }
}
