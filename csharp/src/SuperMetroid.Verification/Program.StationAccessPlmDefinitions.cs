using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies all six station-access PLM header, instruction-list, and draw-pointer translations.</summary>
    /// <param name="rom">Retail address space used to compare each compiled value with its native operand.</param>
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

    /// <summary>Checks that each station-access behavior resolves to its native PLM header.</summary>
    /// <param name="rom">Retail address space containing the room PLM population records.</param>
    private static void VerifyStationAccessHeaders(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 0);

    /// <summary>Checks that each station-access header selects its native initial instruction list.</summary>
    /// <param name="rom">Retail address space containing the header instruction operands.</param>
    private static void VerifyStationAccessInitialLists(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 1);

    /// <summary>Checks compiled draw pointers for the retracted access-block phase.</summary>
    /// <param name="rom">Retail address space containing the retracted draw operands.</param>
    private static void VerifyStationAccessRetractedDraws(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 2);

    /// <summary>Checks compiled draw pointers for the extended access-block phase.</summary>
    /// <param name="rom">Retail address space containing the extended draw operands.</param>
    private static void VerifyStationAccessExtendedDraws(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 3);

    /// <summary>Compares the requested station-access field across the complete BTS byte domain with retail data.</summary>
    /// <param name="rom">Retail address space supplying headers and instruction operands.</param>
    /// <param name="field">Selects header, initial list, retracted draw, or extended draw verification.</param>
    private static void VerifyStationAccessField(SuperMetroidAddressSpace rom, int field)
    {
        var entries = StationAccessPlmDefinitions.All.ToArray();
        AssertEqual(6, entries.Length, "Station access original record count");
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var behavior = (StationAccessBehavior)raw;
            if (raw < 0x47 || raw > 0x4c)
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

    /// <summary>Reads one little-endian instruction or header word from the retail address space.</summary>
    /// <param name="bus">Address space containing the word.</param>
    /// <param name="address">CPU address of its low byte.</param>
    /// <returns>The two bytes combined as a 16-bit word.</returns>
    private static ushort ReadStationAccessWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
