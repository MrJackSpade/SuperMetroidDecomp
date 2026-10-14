using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static RoomPlmPopulationVerification;
using static SuiteTimingVerification;

/// <summary>Checks station-access PLM headers, draw phases, and runtime use of compiled operands.</summary>
internal static class StationAccessPlmDefinitionsVerification
{
    /// <summary>Runs native field checks and a sparse-bus production fixture for map and missile stations.</summary>
    internal static void VerifyStationAccessPlmDefinitions(SuperMetroidAddressSpace rom)
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

    /// <summary>Checks the six access behavior identities through the shared native-field fixture.</summary>
    private static void VerifyStationAccessHeaders(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 0);
    /// <summary>Checks access behavior enumeration while reading the six initial native lists.</summary>
    private static void VerifyStationAccessInitialLists(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 1);
    /// <summary>Checks retracted-phase draw pointers for all six access behaviors.</summary>
    internal static void VerifyStationAccessRetractedDraws(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 2);
    /// <summary>Checks extended-phase draw pointers for all six access behaviors.</summary>
    internal static void VerifyStationAccessExtendedDraws(SuperMetroidAddressSpace rom) => VerifyStationAccessField(rom, 3);

    /// <summary>Checks the full BTS selector domain and compares chosen native operands with compiled definitions.</summary>
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
            ushort header = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x9491c7 + (raw - 0x47) * 2));
            ushort program = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (header + 2)));
            if (field >= 2)
            {
                bool extended = field == 3;
                // Map lists start with a packed three-byte sound command. Resource
                // lists first have a four-byte full-resource branch, then the sound.
                int operand = program + (raw <= 0x48 ? 5 : 9) + (extended ? 4 : 0);
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | operand));
                AssertEqual(expected, extended ? selected.ExtendedDrawPointer : selected.RetractedDrawPointer,
                    "Station native access draw operand");
                AssertEqual(expected, selected.DrawPointer(extended), "Station public phase selection");
                AssertTrue(RoomPlmStationDrawDefinitions.TryGet(expected, out _), "Station selected physical draw is owned");
            }
        }
    }
}
