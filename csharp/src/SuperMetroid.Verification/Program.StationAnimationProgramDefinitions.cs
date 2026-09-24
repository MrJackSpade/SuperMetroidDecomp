using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyStationAnimationProgramDefinitions()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        int count = 0;
        foreach ((ushort address, ushort compiled) in
                 StationAnimationProgramDefinitions.NativeWords())
        {
            ushort native = unchecked((ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8));
            AssertEqual(native, compiled,
                $"station animation word $84:{address:X4} matches pinned ROM");
            count++;
        }

        AssertEqual(30, count, "all 15 station frame durations and draw pointers are compiled");
        AssertThrows<InvalidDataException>(
            () => StationAnimationProgramDefinitions.Resolve(0x8000, 0),
            "unknown station animation list fails loudly");
        AssertThrows<InvalidDataException>(
            () => StationAnimationProgramDefinitions.Resolve(
                StationAnimationProgramDefinitions.MapIdle, 3),
            "station animation cannot read into adjacent code or data");

        // The population test seeds only draw payloads at these native pointers.
        // No duration/draw-pointer words are present in its sparse address space;
        // running its actual map/energy/missile/save station paths therefore proves
        // the production station handler is using this compiled selection table.
        VerifySequentialRoomPlmPopulationLoader();
        Console.WriteLine(
            "Station animations: all 15 frame records match ROM; sparse-bus station activation and save animation use compiled selections.");
    }
}
