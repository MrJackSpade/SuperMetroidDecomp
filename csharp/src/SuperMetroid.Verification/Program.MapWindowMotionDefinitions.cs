using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
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
            var origin = FileSelectMapWindowMotions.StockOrigin(area);
            AssertEqual((int)ReadVerificationWord(rom, 0x81aa1c + area * 4), origin.X,
                "stock motion origin X");
            AssertEqual((int)ReadVerificationWord(rom, 0x81aa1e + area * 4), origin.Y,
                "stock motion origin Y");
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
