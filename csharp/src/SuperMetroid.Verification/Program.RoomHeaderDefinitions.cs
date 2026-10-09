using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Validates the compiled room-header roster and its native fields against the supported retail ROM.</summary>
    private static void VerifyCompiledRoomHeaderDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Room header oracle revision");
        ushort[] rooms = File.ReadLines(Path.GetFullPath(Path.Combine("upstream-sm", "assets", "names.txt")))
            .Select(TryParseRoomHeaderPointer).Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value).Distinct().Order().ToArray();
        Suite(nameof(VerifyRoomHeaderIdentities), () => VerifyRoomHeaderIdentities(rooms));
        Suite(nameof(VerifyRoomHeaderRoomIndex), () => VerifyRoomHeaderRoomIndex(rom, rooms));
        Suite(nameof(VerifyRoomHeaderAreaIndex), () => VerifyRoomHeaderAreaIndex(rom, rooms));
        Suite(nameof(VerifyRoomHeaderMapX), () => VerifyRoomHeaderMapX(rom, rooms));
        Suite(nameof(VerifyRoomHeaderMapY), () => VerifyRoomHeaderMapY(rom, rooms));
        Suite(nameof(VerifyRoomHeaderWidthInScreens), () => VerifyRoomHeaderWidthInScreens(rom, rooms));
        Suite(nameof(VerifyRoomHeaderHeightInScreens), () => VerifyRoomHeaderHeightInScreens(rom, rooms));
        Suite(nameof(VerifyRoomHeaderUpScroller), () => VerifyRoomHeaderUpScroller(rom, rooms));
        Suite(nameof(VerifyRoomHeaderDownScroller), () => VerifyRoomHeaderDownScroller(rom, rooms));
        Suite(nameof(VerifyRoomHeaderCreBitset), () => VerifyRoomHeaderCreBitset(rom, rooms));
        Suite(nameof(VerifyRoomHeaderDoorListPointer), () => VerifyRoomHeaderDoorListPointer(rom, rooms));
        Console.WriteLine("Room headers: all262 identities, ten original native fields, sorted enumeration and complete ushort rejection domain pass.");
    }

    /// <summary>Checks that every native room pointer is represented once and that non-room pointers are rejected.</summary>
    /// <param name="rooms">Distinct, sorted room-header pointers independently extracted from the room-name list.</param>
    private static void VerifyRoomHeaderIdentities(ushort[] rooms)
    {
        AssertEqual(262, rooms.Length, "Independent retail room roster");
        AssertTrue(RoomHeaderDefinitions.All.Select(header => header.Pointer).SequenceEqual(rooms),
            "Header enumeration preserves original sorted identities");
        var known = rooms.ToHashSet();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool expected = known.Contains(pointer);
            AssertEqual(expected, RoomHeaderDefinitionsTooling.Contains(pointer), $"Room membership {pointer:X4}");
            if (expected)
            {
                AssertEqual(pointer, RoomHeaderDefinitions.Get(pointer).Pointer, "Header preserves selected identity");
                AssertEqual(pointer, CartridgeRoomHeader.LoadUsingCompiledSelection(pointer).Pointer,
                    "Production header construction preserves identity without a cartridge capability");
            }
            else
                AssertThrows<ArgumentOutOfRangeException>(() => RoomHeaderDefinitions.Get(pointer),
                    "Non-header address is rejected");
        }
    }

    /// <summary>Compares each room's room-index byte with the corresponding compiled header property.</summary>
    private static void VerifyRoomHeaderRoomIndex(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 0, header => (int)header.RoomIndex);

    /// <summary>Compares each room's area-index byte with the compiled header and production area view.</summary>
    private static void VerifyRoomHeaderAreaIndex(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 1, header => (int)header.AreaIndex);

    /// <summary>Compares each room's map X coordinate byte with its compiled header property.</summary>
    private static void VerifyRoomHeaderMapX(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 2, header => (int)header.MapX);

    /// <summary>Compares each room's map Y coordinate byte with its compiled header property.</summary>
    private static void VerifyRoomHeaderMapY(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 3, header => (int)header.MapY);

    /// <summary>Compares each room's width-in-screens byte with its compiled header property.</summary>
    private static void VerifyRoomHeaderWidthInScreens(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 4, header => (int)header.WidthInScreens);

    /// <summary>Compares each room's height-in-screens byte with its compiled header property.</summary>
    private static void VerifyRoomHeaderHeightInScreens(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 5, header => (int)header.HeightInScreens);

    /// <summary>Compares each room's upward scrolling setting with its compiled header property.</summary>
    private static void VerifyRoomHeaderUpScroller(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 6, header => (int)header.UpScroller);

    /// <summary>Compares each room's downward scrolling setting with its compiled header property.</summary>
    private static void VerifyRoomHeaderDownScroller(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 7, header => (int)header.DownScroller);

    /// <summary>Compares each room's CRE bitset byte with its compiled header property.</summary>
    private static void VerifyRoomHeaderCreBitset(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 8, header => (int)header.CreBitset);

    /// <summary>Compares each room's little-endian door-list pointer with its compiled header property.</summary>
    private static void VerifyRoomHeaderDoorListPointer(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 9, header => (int)header.DoorListPointer);

    /// <summary>Reads a selected native header field from each room and compares it with both compiled header views.</summary>
    /// <param name="rom">Retail address space containing the native bank-$8F room headers.</param>
    /// <param name="rooms">Room pointers whose field values are checked.</param>
    /// <param name="offset">Byte offset of the field within a header; the door-list pointer spans offsets 9 and 10.</param>
    /// <param name="field">Projection that selects the corresponding value from a compiled header.</param>
    private static void VerifyRoomHeaderField(SuperMetroidAddressSpace rom, ushort[] rooms,
        int offset, Func<RoomHeaderDefinition, int> field)
    {
        RoomHeaderDefinition[] enumerated = RoomHeaderDefinitions.All.ToArray();
        for (int index = 0; index < rooms.Length; index++)
        {
            ushort pointer = rooms[index];
            int address = 0x8f0000 | pointer;
            int expected = rom.ReadByte(address + offset);
            if (offset == 9) expected |= rom.ReadByte(address + 10) << 8;
            AssertEqual(expected, field(RoomHeaderDefinitions.Get(pointer)), $"Room {pointer:X4} native field {offset}");
            AssertEqual(expected, field(enumerated[index]), $"Room {pointer:X4} enumerated field {offset}");
            if (offset == 1)
                AssertEqual(expected, (int)CartridgeRoomHeader.ReadAreaIndex(pointer), "Production area view");
        }
    }
}
