using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyStationAnimationProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Station animation oracle revision");
        VerifyStationVisualIds();
        VerifyStationAnimationDurations(rom);
        VerifyStationAnimationDraws(rom);

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
        AssertEqual(20, drawCount, "every station animation and access draw list is compiled");
        AssertTrue(!RoomPlmStationDrawDefinitions.TryGet(0x9a40, out _),
            "adjacent ROM bytes cannot alias a complete station draw list");

        VerifyStationAccessRetractedDraws(rom);
        VerifyStationAccessExtendedDraws(rom);

        // The population fixture carries no station animation, access selection,
        // or draw-list bytes.
        // Its real map, missile, and save paths can finish only with compiled data.
        VerifySequentialRoomPlmPopulationLoader();
        VerifyStationVisuals(rom);
        Console.WriteLine(
            "Station animations: all 15 frame records, 20 draw lists, and 12 access selections match ROM; sparse-bus station activation and save animation use compiled data.");
    }

    private static void VerifyStationAnimationDurations(SuperMetroidAddressSpace rom) => VerifyStationAnimationField(rom, false);
    private static void VerifyStationAnimationDraws(SuperMetroidAddressSpace rom) => VerifyStationAnimationField(rom, true);

    private static void VerifyStationAnimationField(SuperMetroidAddressSpace rom, bool draw)
    {
        ushort[] lists = [0xad66,0xad76,0xadc6,0xae50,0xafe8,0xaffa,0xaffe];
        var words = StationAnimationProgramDefinitions.NativeWords().ToArray();
        AssertEqual(30, words.Length, "Station original enumeration word count");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!lists.Contains((ushort)raw))
                AssertThrows<InvalidDataException>(() => StationAnimationProgramDefinitions.Resolve((ushort)raw, 0), "Station rejected list domain");
        int ordinal = 0;
        for (int listIndex = 0; listIndex < lists.Length; listIndex++)
        {
            ushort list = lists[listIndex];
            int count = listIndex < 4 ? 3 : 1;
            foreach (int bad in new[] { int.MinValue,-1,count,int.MaxValue })
                AssertThrows<InvalidDataException>(() => StationAnimationProgramDefinitions.Resolve(list, bad), "Station exact frame bounds");
            for (int frame = 0; frame < count; frame++, ordinal += 2)
            {
                ushort address = (ushort)(list + frame * 4 + (draw ? 2 : 0));
                ushort expected = ReadStationRomWord(rom, address);
                var actual = StationAnimationProgramDefinitions.Resolve(list, frame);
                AssertEqual(expected, draw ? actual.DrawPointer : actual.Duration, "Station original animation field");
                var enumerated = words[ordinal + (draw ? 1 : 0)];
                AssertEqual(address, enumerated.Address, "Station original field enumeration address/order");
                AssertEqual(expected, enumerated.Value, "Station original enumerated field");
            }
        }
    }

    private static ushort ReadStationRomWord(SuperMetroidAddressSpace rom, int address) =>
        unchecked((ushort)(rom.ReadByte(0x840000 | address) |
            rom.ReadByte(0x840000 | (address + 1)) << 8));
}
