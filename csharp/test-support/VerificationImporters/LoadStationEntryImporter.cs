using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Reads native load-station records only while importing or verifying a cartridge.</summary>
internal static class LoadStationEntryImporter
{
    /// <summary>Follows the area's bank-$80 station table to import its room, door,
    /// camera position, and signed Samus offsets from one native entry.</summary>
    public static NativeLoadStation Load(ISnesAddressSpace bus, AreaId areaIndex, byte stationIndex)
    {
        Ensure.NotNull(bus);
        int areaTableIndex = AreaNumberCodec.Encode(areaIndex);
        ushort listPointer = RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus),
            LoadStationRomData.PointerTable + areaTableIndex * 2);
        int address = 0x800000 | unchecked((ushort)(listPointer + stationIndex * LoadStationRomData.EntryByteCount));
        ushort room = ReadWord(bus, address);
        ushort door = ReadWord(bus, address + 2);
        return new NativeLoadStation(
            Room: room == 0 ? null : RoomHeaderSources.Room(room),
            Door: door == 0 ? null : DoorHeaderSources.Door(door),
            CameraPosition: new(ReadWord(bus, address + 6), ReadWord(bus, address + 8)),
            SamusOffset: new(unchecked((short)ReadWord(bus, address + 12)), unchecked((short)ReadWord(bus, address + 10))));
    }

    /// <summary>Reads one little-endian entry word through the cartridge import boundary.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
}

/// <summary>One native load-station record as the cartridge stores it; an unused slot names no room and no door.</summary>
/// <param name="Room">The room, or null for an unused slot.</param>
/// <param name="Door">The door, or null for an unused slot.</param>
/// <param name="CameraPosition">The initial camera top-left.</param>
/// <param name="SamusOffset">Samus's placement relative to the camera.</param>
internal sealed record NativeLoadStation(
    RoomHeaderDefinition? Room, DoorDefinition? Door, Position16<RoomPixels> CameraPosition, Offset16<RoomPixels> SamusOffset)
{
    /// <summary>The room position the record places Samus at, wrapping each axis at sixteen bits.</summary>
    public Position16<RoomPixels> SamusPosition => CameraPosition + LoadStationPlacement.SamusOffsetOrigin + SamusOffset;
}
