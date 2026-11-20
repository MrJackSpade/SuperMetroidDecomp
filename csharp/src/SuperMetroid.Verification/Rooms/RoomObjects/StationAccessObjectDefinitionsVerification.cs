using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectPopulationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks station-access PLM headers, draw phases, and runtime use of compiled operands.</summary>
internal static class StationAccessObjectDefinitionsVerification
{
    /// <summary>Runs native field checks and a sparse-bus production fixture for map and missile stations.</summary>
    internal static void VerifyStationAccessObjectDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyStationAccessHeaders), () => VerifyStationAccessHeaders(rom));
        Suite(nameof(VerifyStationAccessInitialLists), () => VerifyStationAccessInitialLists(rom));
        Suite(nameof(VerifyStationAccessRetractedDraws), () => VerifyStationAccessRetractedDraws(rom));
        Suite(nameof(VerifyStationAccessExtendedDraws), () => VerifyStationAccessExtendedDraws(rom));

        // This production fixture activates map and missile access blocks and draws
        // both phases. Its sparse bus intentionally omits all six header+2 words.
        Suite(nameof(VerifySequentialRoomObjectPopulationLoader), () => VerifySequentialRoomObjectPopulationLoader());
        Console.WriteLine(
            "Station access PLMs: all six native access draw operands and real access animations pass without runtime header reads.");
    }

    /// <summary>Checks the six access behavior identities through the shared native-field fixture.</summary>
    private static void VerifyStationAccessHeaders(CartridgeImportAddressSpace rom) => VerifyStationAccessField(rom, 0);
    /// <summary>Checks access behavior enumeration while reading the six initial native lists.</summary>
    private static void VerifyStationAccessInitialLists(CartridgeImportAddressSpace rom) => VerifyStationAccessField(rom, 1);
    /// <summary>Checks retracted-phase draw pointers for all six access behaviors.</summary>
    internal static void VerifyStationAccessRetractedDraws(CartridgeImportAddressSpace rom) => VerifyStationAccessField(rom, 2);
    /// <summary>Checks extended-phase draw pointers for all six access behaviors.</summary>
    internal static void VerifyStationAccessExtendedDraws(CartridgeImportAddressSpace rom) => VerifyStationAccessField(rom, 3);

    /// <summary>The six station accesses in the cartridge's order of their behaviours: each station's right side, then its left.</summary>
    private static readonly StationAccess[] NativeOrder =
    [
        new(StationKind.Map, HorizontalDirection.Right), new(StationKind.Map, HorizontalDirection.Left),
        new(StationKind.Energy, HorizontalDirection.Right), new(StationKind.Energy, HorizontalDirection.Left),
        new(StationKind.Missile, HorizontalDirection.Right), new(StationKind.Missile, HorizontalDirection.Left),
    ];

    /// <summary>Checks the full BTS selector domain and compares chosen native operands with compiled definitions.</summary>
    private static void VerifyStationAccessField(CartridgeImportAddressSpace rom, int field)
    {
        AssertEqual(NativeOrder.Length, StationAccessObjectDefinitions.Arms.Count, "Station access original record count");
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            RoomBlockBehavior decoded = RoomBlockBehaviorCodec.Decode(raw);
            bool decodes = decoded.TryGetStationAccess(out StationAccess access);
            if (raw is < 0x47 or > 0x4c)
            {
                // Outside the six accesses nothing decodes as one; the save floor, just past them, is a reaction of its
                // own and selects no access PLM.
                AssertTrue(!decodes, "Station access full rejected BTS domain");
                AssertEqual(raw == 0x4d, decoded.IsSaveStationFloor, "Only the save floor's BTS is the save station's floor");
                continue;
            }
            AssertTrue(decodes, $"Station access BTS ${raw:X2} decodes");
            AssertEqual(NativeOrder[raw - 0x47], access, "Station access original enumeration order");
            StationAccessObjectDefinition selected = StationAccessObjectDefinitions.Arms[access];
            ushort header = rom.ReadCartridgeWord((0x9491c7 + (raw - 0x47) * 2));
            ushort program = rom.ReadCartridgeWord((0x840000 | (header + 2)));
            if (field >= 2)
            {
                bool extended = field == 3;
                // Map lists start with a packed three-byte sound command. Resource
                // lists first have a four-byte full-resource branch, then the sound.
                int operand = program + (raw <= 0x48 ? 5 : 9) + (extended ? 4 : 0);
                ushort expected = rom.ReadCartridgeWord((0x840000 | operand));
                AssertEqual(expected, RoomObjectDrawAddressCodec.Of(StationObjectDraws.Family.Of(extended ? selected.Extended : selected.Retracted)),
                    "Station native access draw operand");
                AssertEqual(expected, RoomObjectDrawAddressCodec.Of(StationObjectDraws.Family.Of(extended ? selected.Extended : selected.Retracted)), "Station public phase selection");
                AssertTrue(RoomObjectStationVisualFiles.TryGet(NativeObjectDraws.At<StationDraw>(expected), out _), "Station selected physical draw is owned");
            }
        }
    }
}
