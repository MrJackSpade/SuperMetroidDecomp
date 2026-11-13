using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks area-specific file-select map window origins, edge velocities, and timers.</summary>
internal static class MapWindowMotionDefinitionsVerification
{
    /// <summary>Compares native X origins with the stock and installed label positions.</summary>
    internal static void VerifyMapWindowOriginX(ISnesAddressSpace rom, WorldMapLabelPositions labels) =>
        Suite(nameof(VerifyMapWindowOrigin), () => VerifyMapWindowOrigin(rom, labels, vertical: false));
    /// <summary>Compares native Y origins with the stock and installed label positions.</summary>
    internal static void VerifyMapWindowOriginY(ISnesAddressSpace rom, WorldMapLabelPositions labels) =>
        Suite(nameof(VerifyMapWindowOrigin), () => VerifyMapWindowOrigin(rom, labels, vertical: true));

    /// <summary>Checks one coordinate for all six areas and rejects unsupported identities.</summary>
    private static void VerifyMapWindowOrigin(ISnesAddressSpace rom, WorldMapLabelPositions labels, bool vertical)
    {
        int field = vertical ? 2 : 0;
        for (int area = 0; area < 6; area++)
        {
            int expected = ReadVerificationWord(rom, 0x81aa1c + area * 4 + field);
            var stock = FileSelectMapWindowMotions.StockOrigin(area);
            AssertEqual(expected, vertical ? stock.Y : stock.X, $"native area {area} origin field {field}");
            Position16<SpritePixels> installed = labels.Get(area);
            AssertEqual(expected, vertical ? installed.Y : installed.X, $"imported area {area} origin field {field}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 6, 7, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapWindowMotions.StockOrigin(invalid),
                "unsupported stock origin area");
            AssertThrows<ArgumentOutOfRangeException>(() => labels.Get(invalid), "unsupported installed label area");
        }
    }

    /// <summary>Compares native signed 16.16 velocities for the left window edge.</summary>
    internal static void VerifyMapWindowLeftVelocities(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyMapWindowVelocity), () => VerifyMapWindowVelocity(rom, 0, motion => motion.Left));
    /// <summary>Compares native signed 16.16 velocities for the right window edge.</summary>
    internal static void VerifyMapWindowRightVelocities(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyMapWindowVelocity), () => VerifyMapWindowVelocity(rom, 1, motion => motion.Right));
    /// <summary>Compares native signed 16.16 velocities for the top window edge.</summary>
    internal static void VerifyMapWindowTopVelocities(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyMapWindowVelocity), () => VerifyMapWindowVelocity(rom, 2, motion => motion.Top));
    /// <summary>Compares native signed 16.16 velocities for the bottom window edge.</summary>
    internal static void VerifyMapWindowBottomVelocities(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyMapWindowVelocity), () => VerifyMapWindowVelocity(rom, 3, motion => motion.Bottom));

    /// <summary>Checks one edge's velocity record for each area and exercises area bounds.</summary>
    private static void VerifyMapWindowVelocity(ISnesAddressSpace rom, int edge,
        Func<FileSelectMapWindowMotion, SignedSixteenSixteen> select)
    {
        for (int area = 0; area < 6; area++)
        {
            int address = 0x81aa34 + area * 16 + edge * 4;
            uint expected = SixteenSixteenWords.Compose(ReadVerificationWord(rom, address + 2), ReadVerificationWord(rom, address));
            AssertEqual(unchecked((int)expected), select(FileSelectMapWindowMotions.Get(area)).RawValue,
                $"map window area {area} edge {edge} signed16.16 velocity");
        }
        Suite(nameof(VerifyMapWindowMotionBounds), () => VerifyMapWindowMotionBounds());
    }

    /// <summary>
    /// Compares the opening's moving updates for all six area windows with the cartridge's countdown: the window moves on
    /// the countdown's start value and on the update that reaches zero.
    /// </summary>
    internal static void VerifyMapWindowTimers(ISnesAddressSpace rom)
    {
        for (int area = 0; area < 6; area++)
            AssertEqual(ReadVerificationWord(rom, 0x81aa94 + area * 2) + 1,
                FileSelectMapWindowMotions.Get(area).MovingUpdates, $"map window area {area} moving updates");
        Suite(nameof(VerifyMapWindowMotionBounds), () => VerifyMapWindowMotionBounds());
    }

    /// <summary>Requires the motion catalog to reject area indices outside the six retail areas.</summary>
    private static void VerifyMapWindowMotionBounds()
    {
        foreach (int area in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapWindowMotions.Get(area),
                $"unsupported map window area {area}");
    }

    /// <summary>
    /// Independent cartridge-data control, deliberately outside the production catalog: the cartridge's countdown moves the
    /// window on its start value and on the update that reaches zero.
    /// </summary>
    internal static FileSelectMapWindowMotion ReadCartridgeMapWindowMotion(ISnesAddressSpace bus, int area)
    {
        SignedSixteenSixteen ReadVelocity(int edge)
        {
            int address = FileSelectMapNativeSources.WindowVelocities + area * FileSelectMapNativeSources.VelocityRecordBytes + edge * 4;
            return SixteenSixteenWords.ToDisplacement(unchecked((int)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address) |
                (SixteenSixteenWords.FromWhole(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2))))));
        }
        return new(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapNativeSources.WindowTimers + area * 2) + 1,
            ReadVelocity(0), ReadVelocity(1), ReadVelocity(2), ReadVelocity(3));
    }
}
