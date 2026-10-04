using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMapWindowOriginX(ISnesAddressSpace rom, WorldMapLabelLayout labels) =>
        VerifyMapWindowOrigin(rom, labels, vertical: false);
    private static void VerifyMapWindowOriginY(ISnesAddressSpace rom, WorldMapLabelLayout labels) =>
        VerifyMapWindowOrigin(rom, labels, vertical: true);

    private static void VerifyMapWindowOrigin(ISnesAddressSpace rom, WorldMapLabelLayout labels, bool vertical)
    {
        int field = vertical ? 2 : 0;
        for (int area = 0; area < 6; area++)
        {
            int expected = ReadVerificationWord(rom, 0x81aa1c + area * 4 + field);
            var stock = FileSelectMapWindowMotions.StockOrigin(area);
            AssertEqual(expected, vertical ? stock.Y : stock.X, $"native area {area} origin field {field}");
            MapLabelPoint installed = labels.Get(area);
            AssertEqual(expected, vertical ? installed.Y : installed.X, $"imported area {area} origin field {field}");

            var editedAreas = new Dictionary<string, MapLabelPoint>();
            for (int identity = 0; identity < 6; identity++)
                editedAreas.Add(((AreaId)identity).ToString(), labels.Get(identity));
            int editedCoordinate = vertical ? 223 : 255;
            editedAreas[((AreaId)area).ToString()] = vertical
                ? installed with { Y = editedCoordinate } : installed with { X = editedCoordinate };
            using var json = new MemoryStream();
            WorldMapLabelLayout.Write(json, new() { Version = 1, Areas = editedAreas });
            json.Position = 0;
            WorldMapLabelLayout edited = WorldMapLabelLayout.Load(json);
            for (int identity = 0; identity < 6; identity++)
                AssertEqual(editedAreas[((AreaId)identity).ToString()], edited.Get(identity),
                    "named selection preserves edited and untouched coordinate fields");
            var window = new FileSelectMapWindow(rom, area, edited);
            AssertEqual(edited.Get(area).X, (int)window.Left, "actual window uses editable X");
            AssertEqual(edited.Get(area).Y, (int)window.Top, "actual window uses editable Y");
            AssertEqual(window.Left, window.Right, "edited window starts with zero width");
            AssertEqual(window.Top, window.Bottom, "edited window starts with zero height");
            AssertEqual(stock, FileSelectMapWindowMotions.StockOrigin(area), "editing labels preserves stock motion input");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 6, 7, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapWindowMotions.StockOrigin(invalid),
                "unsupported stock origin area");
            AssertThrows<ArgumentOutOfRangeException>(() => labels.Get(invalid), "unsupported installed label area");
        }
    }

    private static void VerifyMapWindowLeftVelocities(ISnesAddressSpace rom) =>
        VerifyMapWindowVelocity(rom, 0, motion => motion.Left);
    private static void VerifyMapWindowRightVelocities(ISnesAddressSpace rom) =>
        VerifyMapWindowVelocity(rom, 1, motion => motion.Right);
    private static void VerifyMapWindowTopVelocities(ISnesAddressSpace rom) =>
        VerifyMapWindowVelocity(rom, 2, motion => motion.Top);
    private static void VerifyMapWindowBottomVelocities(ISnesAddressSpace rom) =>
        VerifyMapWindowVelocity(rom, 3, motion => motion.Bottom);

    private static void VerifyMapWindowVelocity(ISnesAddressSpace rom, int edge,
        Func<FileSelectMapWindowMotion, uint> select)
    {
        for (int area = 0; area < 6; area++)
        {
            int address = 0x81aa34 + area * 16 + edge * 4;
            uint expected = ReadVerificationWord(rom, address) |
                ((uint)ReadVerificationWord(rom, address + 2) << 16);
            AssertEqual(expected, select(FileSelectMapWindowMotions.Get(area)),
                $"map window area {area} edge {edge} signed16.16 velocity");
        }
        VerifyMapWindowMotionBounds();
    }

    private static void VerifyMapWindowTimers(ISnesAddressSpace rom)
    {
        for (int area = 0; area < 6; area++)
            AssertEqual(ReadVerificationWord(rom, 0x81aa94 + area * 2),
                FileSelectMapWindowMotions.Get(area).Timer, $"map window area {area} timer");
        VerifyMapWindowMotionBounds();
    }

    private static void VerifyMapWindowMotionBounds()
    {
        foreach (int area in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapWindowMotions.Get(area),
                $"unsupported map window area {area}");
    }

    /// <summary>Independent cartridge-data control, deliberately outside the production catalog.</summary>
    private static FileSelectMapWindowMotion ReadCartridgeMapWindowMotion(ISnesAddressSpace bus, int area)
    {
        uint ReadVelocity(int edge)
        {
            int address = FileSelectMapRomData.WindowVelocities + area * FileSelectMapRomData.VelocityRecordBytes + edge * 4;
            return RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address) |
                ((uint)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2) << 16);
        }
        return new(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.WindowTimers + area * 2),
            ReadVelocity(0), ReadVelocity(1), ReadVelocity(2), ReadVelocity(3));
    }
}
