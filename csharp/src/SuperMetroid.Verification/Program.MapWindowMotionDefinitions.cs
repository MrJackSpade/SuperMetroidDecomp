using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
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
