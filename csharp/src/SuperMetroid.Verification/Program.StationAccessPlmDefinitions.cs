using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyStationAccessPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyStationAccessHeaders), () => VerifyStationAccessHeaders(rom));
        Suite(nameof(VerifyStationAccessInitialLists), () => VerifyStationAccessInitialLists(rom));
        Suite(nameof(VerifyStationAccessRetractedDraws), () => VerifyStationAccessRetractedDraws(rom));
        Suite(nameof(VerifyStationAccessExtendedDraws), () => VerifyStationAccessExtendedDraws(rom));

        // This production fixture activates map and missile access blocks and draws
        // both phases. Its sparse bus intentionally omits all six header+2 words.
        Suite(nameof(VerifySequentialRoomPlmPopulationLoader), () => VerifySequentialRoomPlmPopulationLoader());
        Console.WriteLine(
            "Station access PLMs: all six native access draw operands and real access animations pass without runtime header reads.");
    }

    private static void VerifyStationAccessHeaders(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 0);
    private static void VerifyStationAccessInitialLists(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 1);
    private static void VerifyStationAccessRetractedDraws(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 2);
    private static void VerifyStationAccessExtendedDraws(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 3);

    private static void VerifyStationAccessField(SuperMetroidAddressSpace rom, int field)
    {
        var entries = StationAccessPlmDefinitions.All.ToArray();
        AssertEqual(6, entries.Length, "Station access original record count");
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var behavior = (StationAccessBehavior)raw;
            if (raw is < 0x47 or > 0x4c)
            {
                AssertThrows<InvalidDataException>(() => StationAccessPlmDefinitions.Resolve(behavior), "Station access full rejected BTS domain");
                continue;
            }
            var selected = StationAccessPlmDefinitions.Resolve(behavior);
            AssertEqual(selected, entries[raw - 0x47], "Station access original enumeration order");
            ushort header = ReadStationAccessWord(rom, 0x9491c7 + (raw - 0x47) * 2);
            ushort program = ReadStationAccessWord(rom, 0x840000 | (header + 2));
            if (field >= 2)
            {
                bool extended = field == 3;
                // Map lists start with a packed three-byte sound command. Resource
                // lists first have a four-byte full-resource branch, then the sound.
                int operand = program + (raw <= 0x48 ? 5 : 9) + (extended ? 4 : 0);
                ushort expected = ReadStationAccessWord(rom, 0x840000 | operand);
                AssertEqual(expected, extended ? selected.ExtendedDrawPointer : selected.RetractedDrawPointer,
                    "Station native access draw operand");
                AssertEqual(expected, selected.DrawPointer(extended), "Station public phase selection");
                AssertTrue(RoomPlmStationDrawDefinitions.TryGet(expected, out _), "Station selected physical draw is owned");
            }
        }
    }

    private static ushort ReadStationAccessWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
