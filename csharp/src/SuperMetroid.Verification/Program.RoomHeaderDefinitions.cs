using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledRoomHeaderDefinitions()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
            AssertEqual(expected, RoomHeaderDefinitions.Contains(pointer), $"Room membership {pointer:X4}");
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

    private static void VerifyRoomHeaderRoomIndex(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 0, header => (int)header.RoomIndex);
    private static void VerifyRoomHeaderAreaIndex(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 1, header => (int)header.AreaIndex);
    private static void VerifyRoomHeaderMapX(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 2, header => (int)header.MapX);
    private static void VerifyRoomHeaderMapY(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 3, header => (int)header.MapY);
    private static void VerifyRoomHeaderWidthInScreens(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 4, header => (int)header.WidthInScreens);
    private static void VerifyRoomHeaderHeightInScreens(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 5, header => (int)header.HeightInScreens);
    private static void VerifyRoomHeaderUpScroller(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 6, header => (int)header.UpScroller);
    private static void VerifyRoomHeaderDownScroller(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 7, header => (int)header.DownScroller);
    private static void VerifyRoomHeaderCreBitset(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 8, header => (int)header.CreBitset);
    private static void VerifyRoomHeaderDoorListPointer(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 9, header => (int)header.DoorListPointer);

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
