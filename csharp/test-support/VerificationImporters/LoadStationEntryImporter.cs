using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Reads native load-station records only while importing or verifying a cartridge.</summary>
[CpuBusBoundary("codec: ROM bank-$80 load-station pointer table and entries → LoadStationEntry")]
internal static class LoadStationEntryImporter
{
    /// <summary>Follows the area's bank-$80 station table to import its room, door,
    /// camera position, and signed Samus offsets from one native entry.</summary>
    public static LoadStationEntry Load(ISnesAddressSpace bus, AreaId areaIndex, byte stationIndex)
    {
        Ensure.NotNull(bus);
        int areaTableIndex = AreaIds.ToIndex(areaIndex);
        ushort listPointer = RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus),
            LoadStationRomData.PointerTable + areaTableIndex * 2);
        int address = 0x800000 | unchecked((ushort)(listPointer + stationIndex * LoadStationRomData.EntryByteCount));
        ushort room = ReadWord(bus, address);
        ushort door = ReadWord(bus, address + 2);
        return new LoadStationEntry(
            Room: room == 0 ? null : RoomHeaderSources.Room(room),
            Door: door == 0 ? null : DoorHeaderSources.Door(door),
            CameraPosition: new(ReadWord(bus, address + 6), ReadWord(bus, address + 8)),
            SamusOffset: new(unchecked((short)ReadWord(bus, address + 12)), unchecked((short)ReadWord(bus, address + 10))));
    }

    /// <summary>Reads one little-endian entry word through the cartridge import boundary.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
}
