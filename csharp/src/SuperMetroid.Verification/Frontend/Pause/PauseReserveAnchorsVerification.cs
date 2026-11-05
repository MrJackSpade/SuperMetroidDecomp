using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks reserve tank OAM anchors against cartridge coordinates.</summary>
internal static class PauseReserveAnchorsVerification
{
    /// <summary>Checks stock positions, invalid coordinates, and anchor bounds.</summary>
    internal static void VerifyPauseReserveAnchors(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseReserveTankExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveTankDocument>(bytes, ArtworkJson.Options);
        var stock = PauseReserveTankSourcePresentation.Load(new MemoryStream(bytes));
        AssertEqual(0, stock.StoredAnchorComponentCount, "stock reserve coordinates are calculated");
        Suite(nameof(VerifyPauseReserveAnchorField), () => VerifyPauseReserveAnchorField(rom, document, stock, true));
        Suite(nameof(VerifyPauseReserveAnchorField), () => VerifyPauseReserveAnchorField(rom, document, stock, false));
        foreach (int invalid in new[] { int.MinValue, -1, 6, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Anchor(invalid), "reserve anchor array boundary preserved");
    }

    /// <summary>Checks one anchor coordinate axis against native words and rejects out-of-range coordinates.</summary>
    private static void VerifyPauseReserveAnchorField(ISnesAddressSpace rom, PauseReserveTankDocument document,
        PauseReserveTankPresentation stock, bool horizontal)
    {
        for (int index = 0; index < 6; index++)
        {
            int expected = horizontal ? ReadVerificationWord(rom, 0x82c1d6 + index * 2) : ReadVerificationWord(rom, 0x82c1e2) - 1;
            var point = stock.Anchor(index);
            var basis = PauseReserveTankParts.StockAnchor(index);
            AssertEqual(expected, horizontal ? point.X : point.Y, "original installed reserve coordinate");
            AssertEqual(expected, horizontal ? basis.X : basis.Y, "original reserve geometry");
        }
        foreach (int invalid in new[] { -1, horizontal ? 256 : 224 })
        {
            var anchors = (Position16<SpritePixels>[])document.Anchors.Clone();
            anchors[0] = horizontal ? new(unchecked((ushort)invalid), anchors[0].Y) : new(anchors[0].X, unchecked((ushort)invalid));
            AssertThrows<InvalidDataException>(() =>
            {
                using var written = new MemoryStream();
                PauseReserveTankSourcePresentation.Write(written, document with { Anchors = anchors });
                _ = PauseReserveTankSourcePresentation.Load(new MemoryStream(written.ToArray(), writable: false));
            }, "invalid authored reserve coordinate on read");
        }
    }
}
