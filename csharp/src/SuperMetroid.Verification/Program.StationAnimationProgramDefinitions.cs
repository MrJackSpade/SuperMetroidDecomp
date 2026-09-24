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

        int drawCount = 0;
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmStationDrawDefinitions.All)
        {
            int cursor = list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.DirectionAndCount,
                    ReadStationRomWord(rom, cursor),
                    $"station draw ${list.Pointer:X4} direction/count");
                cursor += 2;
                foreach (ushort word in run.LevelWords.Span)
                {
                    AssertEqual(word, ReadStationRomWord(rom, cursor),
                        $"station draw ${list.Pointer:X4} physical level word");
                    cursor += 2;
                }

                AssertEqual(unchecked((byte)run.NextX), rom.ReadByte(0x840000 | cursor++),
                    $"station draw ${list.Pointer:X4} next X");
                AssertEqual(unchecked((byte)run.NextY), rom.ReadByte(0x840000 | cursor++),
                    $"station draw ${list.Pointer:X4} next Y");
            }

            drawCount++;
        }
        AssertEqual(12, drawCount, "every station animation draw list is compiled");
        AssertTrue(!RoomPlmStationDrawDefinitions.TryGet(0x9a40, out _),
            "adjacent ROM bytes cannot alias a complete station draw list");

        // The population fixture carries no station animation or draw-list bytes.
        // Its real map, missile, and save paths can finish only with compiled data.
        VerifySequentialRoomPlmPopulationLoader();
        Console.WriteLine(
            "Station animations: all 15 frame records and 12 draw lists match ROM; sparse-bus station activation and save animation use compiled data.");
    }

    private static ushort ReadStationRomWord(SuperMetroidAddressSpace rom, int address) =>
        unchecked((ushort)(rom.ReadByte(0x840000 | address) |
            rom.ReadByte(0x840000 | (address + 1)) << 8));
}
