using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the compiled room roster and header fields against the pinned retail cartridge.</summary>
internal static class RoomHeaderDefinitionsVerification
{
    /// <summary>Checks all 262 named room identities and ten native fields, including sorted enumeration.</summary>
    internal static void VerifyCompiledRoomHeaderDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Room header oracle revision");
        ushort[] rooms = File.ReadLines(Path.GetFullPath(Path.Combine("upstream-sm", "assets", "names.txt")))
            .Select(TryParseRoomHeaderPointer).Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer."))).Distinct().Order().ToArray();
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

    /// <summary>Checks every ushort pointer for roster membership and production header construction.</summary>
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

    /// <summary>Checks the room index stored at header byte zero.</summary>
    private static void VerifyRoomHeaderRoomIndex(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 0, header => (int)header.RoomIndex);
    /// <summary>Checks the area byte and its production area lookup.</summary>
    private static void VerifyRoomHeaderAreaIndex(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 1, header => (int)header.AreaIndex);
    /// <summary>Checks each room's horizontal map coordinate against header byte two.</summary>
    private static void VerifyRoomHeaderMapX(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 2, header => header.MapPosition.X);
    /// <summary>Checks each room's vertical map coordinate against header byte three.</summary>
    private static void VerifyRoomHeaderMapY(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 3, header => header.MapPosition.Y);
    /// <summary>Checks room width in screen units against header byte four.</summary>
    private static void VerifyRoomHeaderWidthInScreens(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 4, header => header.Size.X);
    /// <summary>Checks room height in screen units against header byte five.</summary>
    private static void VerifyRoomHeaderHeightInScreens(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 5, header => header.Size.Y);
    /// <summary>Checks the upward scroller setting at header byte six.</summary>
    private static void VerifyRoomHeaderUpScroller(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 6, header => (int)header.UpScroller);
    /// <summary>Checks the downward scroller setting at header byte seven.</summary>
    private static void VerifyRoomHeaderDownScroller(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 7, header => (int)header.DownScroller);
    /// <summary>Checks the CRE selection bits at header byte eight.</summary>
    private static void VerifyRoomHeaderCreBitset(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 8, header => (int)header.CreBitset);
    /// <summary>Checks the two-byte door-list pointer at the end of each header.</summary>
    private static void VerifyRoomHeaderDoorListPointer(SuperMetroidAddressSpace rom, ushort[] rooms) => VerifyRoomHeaderField(rom, rooms, 9, header => (int)header.DoorListPointer);

    /// <summary>Compares one selected header field in direct and enumerated views with its cartridge bytes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for room header field")]
    private static void VerifyRoomHeaderField(SuperMetroidAddressSpace rom, ushort[] rooms,
        int offset, Func<RoomHeaderDefinition, int> field)
    {
        RoomHeaderDefinition[] enumerated = RoomHeaderDefinitions.All.ToArray();
        for (int index = 0; index < rooms.Length; index++)
        {
            ushort pointer = rooms[index];
            int address = 0x8f0000 | pointer;
            int expected = rom.ReadCartridgeByte(address + offset);
            if (offset == 9) expected |= rom.ReadCartridgeByte(address + 10) << 8;
            AssertEqual(expected, field(RoomHeaderDefinitions.Get(pointer)), $"Room {pointer:X4} native field {offset}");
            AssertEqual(expected, field(enumerated[index]), $"Room {pointer:X4} enumerated field {offset}");
            if (offset == 1)
                AssertEqual(expected, (int)CartridgeRoomHeader.ReadAreaIndex(pointer), "Production area view");
        }
    }

    /// <summary>Runs the compiled room-header verification suite.</summary>
    [VerificationCommand("--room-header-definitions")]
    internal static int RoomHeaderDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledRoomHeaderDefinitions), () => VerifyCompiledRoomHeaderDefinitions());
        return 0;
    }
}
