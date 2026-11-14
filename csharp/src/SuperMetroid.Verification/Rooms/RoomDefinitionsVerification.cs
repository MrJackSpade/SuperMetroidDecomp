using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks the compiled room roster and each room definition's fields against the pinned retail cartridge's room headers.</summary>
internal static class RoomDefinitionsVerification
{
    /// <summary>Checks all 261 named room identities and ten native fields, including sorted enumeration.</summary>
    internal static void VerifyCompiledRoomDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Room header oracle revision");
        ushort[] rooms = File.ReadLines(Path.GetFullPath(Path.Combine("upstream-sm", "assets", "names.txt")))
            .Select(TryParseRoomHeaderPointer).Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer."))).Distinct().Order().ToArray();
        Suite(nameof(VerifyRoomDefinitionIdentities), () => VerifyRoomDefinitionIdentities(rooms));
        Suite(nameof(VerifyRoomDefinitionRoomIndex), () => VerifyRoomDefinitionRoomIndex(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionArea), () => VerifyRoomDefinitionArea(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionMapX), () => VerifyRoomDefinitionMapX(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionMapY), () => VerifyRoomDefinitionMapY(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionWidthInScreens), () => VerifyRoomDefinitionWidthInScreens(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionHeightInScreens), () => VerifyRoomDefinitionHeightInScreens(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionUpScroller), () => VerifyRoomDefinitionUpScroller(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionDownScroller), () => VerifyRoomDefinitionDownScroller(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionIsolatedDoorTransition), () => VerifyRoomDefinitionIsolatedDoorTransition(rom, rooms));
        Suite(nameof(VerifyRoomDefinitionDoorListPointer), () => VerifyRoomDefinitionDoorListPointer(rom, rooms));
        Console.WriteLine("Room definitions: all 261 identities, ten fields against the cartridge's headers, sorted enumeration and complete ushort rejection domain pass.");
    }

    /// <summary>Checks every ushort pointer for roster membership and production header construction.</summary>
    private static void VerifyRoomDefinitionIdentities(ushort[] rooms)
    {
        AssertEqual(261, rooms.Length, "Independent retail room roster");
        AssertTrue(RetailRooms.All.Select(header => header.Pointer).SequenceEqual(rooms),
            "Header enumeration preserves original sorted identities");
        var known = rooms.ToHashSet();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool expected = known.Contains(pointer);
            AssertEqual(expected, RoomDefinitions.Contains(pointer), $"Room membership {pointer:X4}");
            if (expected)
            {
                AssertEqual(pointer, RoomDefinitions.Get(pointer).Pointer, "Header preserves selected identity");
                AssertEqual(pointer, SelectedRoom.LoadUsingCompiledSelection(RoomDefinitions.Get(pointer)).Pointer,
                    "Production header construction preserves identity without a cartridge capability");
            }
            else
                AssertThrows<ArgumentOutOfRangeException>(() => RoomDefinitions.Get(pointer),
                    "Non-header address is rejected");
        }
    }

    /// <summary>Checks the room index stored at header byte zero.</summary>
    private static void VerifyRoomDefinitionRoomIndex(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 0, header => header.RoomIndex);
    /// <summary>Checks the area byte and its production area lookup.</summary>
    private static void VerifyRoomDefinitionArea(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 1, header => SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(header.Area));
    /// <summary>Checks each room's horizontal map coordinate against header byte two.</summary>
    private static void VerifyRoomDefinitionMapX(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 2, header => header.MapPosition.X);
    /// <summary>Checks each room's vertical map coordinate against header byte three.</summary>
    private static void VerifyRoomDefinitionMapY(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 3, header => header.MapPosition.Y);
    /// <summary>Checks room width in screen units against header byte four.</summary>
    private static void VerifyRoomDefinitionWidthInScreens(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 4, header => header.Size.X);
    /// <summary>Checks room height in screen units against header byte five.</summary>
    private static void VerifyRoomDefinitionHeightInScreens(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 5, header => header.Size.Y);
    /// <summary>Checks the upward scroller setting at header byte six.</summary>
    private static void VerifyRoomDefinitionUpScroller(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 6, header => header.UpScroller);
    /// <summary>Checks the downward scroller setting at header byte seven.</summary>
    private static void VerifyRoomDefinitionDownScroller(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 7, header => header.DownScroller);
    /// <summary>
    /// Checks the isolated door transition against bit zero of header byte eight, whatever the byte's other bits hold:
    /// rooms that set only other bits (the common-element tile reloads) keep the ordinary transition.
    /// </summary>
    private static void VerifyRoomDefinitionIsolatedDoorTransition(CartridgeImportAddressSpace rom, ushort[] rooms)
    {
        bool sawOtherBitsOnly = false;
        foreach (ushort pointer in rooms)
        {
            int settings = rom.ReadCartridgeByte((0x8f0000 | pointer) + 8);
            bool expected = (settings & 1) != 0;
            AssertEqual(expected, RoomDefinitions.Get(pointer).UsesIsolatedDoorTransition,
                $"Room {pointer:X4} settings ${settings:X2} isolated door transition");
            if (!expected && settings != 0) sawOtherBitsOnly = true;
        }
        AssertTrue(sawOtherBitsOnly, "a room whose only header settings bits are other bits keeps the ordinary transition");
    }
    /// <summary>Checks the two-byte door-list pointer at the end of each header.</summary>
    private static void VerifyRoomDefinitionDoorListPointer(CartridgeImportAddressSpace rom, ushort[] rooms) => VerifyRoomDefinitionField(rom, rooms, 9, header => RoomDoorListDefinitions.ListPointerOf(header));

    /// <summary>Compares one selected header field in direct and enumerated views with its cartridge bytes.</summary>
    private static void VerifyRoomDefinitionField(CartridgeImportAddressSpace rom, ushort[] rooms,
        int offset, Func<RoomDefinition, int> field)
    {
        RoomDefinition[] enumerated = RetailRooms.All.ToArray();
        for (int index = 0; index < rooms.Length; index++)
        {
            ushort pointer = rooms[index];
            int address = 0x8f0000 | pointer;
            int expected = rom.ReadCartridgeByte(address + offset);
            if (offset == 9) expected |= rom.ReadCartridgeByte(address + 10) << 8;
            AssertEqual(expected, field(RoomDefinitions.Get(pointer)), $"Room {pointer:X4} native field {offset}");
            AssertEqual(expected, field(enumerated[index]), $"Room {pointer:X4} enumerated field {offset}");
            if (offset == 1)
                AssertEqual(expected, SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(SelectedRoom.LoadUsingCompiledSelection(RoomDefinitions.Get(pointer)).Area), "Production area view");
        }
    }

    /// <summary>Runs the compiled room definition verification suite.</summary>
    [VerificationCommand("--room-definitions")]
    internal static int RoomDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledRoomDefinitions), () => VerifyCompiledRoomDefinitions());
        return 0;
    }
}
