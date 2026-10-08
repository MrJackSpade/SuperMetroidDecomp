using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads native load-station records only while importing or verifying a cartridge.</summary>
internal static class LoadStationEntryImporter
{
    public static LoadStationEntry Load(ISnesAddressSpace bus, AreaId areaIndex, byte stationIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int areaTableIndex = AreaIds.ToIndex(areaIndex);
        ushort listPointer = RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus),
            LoadStationRomData.PointerTable + areaTableIndex * 2);
        int address = 0x800000 | unchecked((ushort)(listPointer + stationIndex * LoadStationRomData.EntryByteCount));
        return new LoadStationEntry(
            RoomPointer: ReadWord(bus, address),
            DoorPointer: ReadWord(bus, address + 2),
            CameraX: ReadWord(bus, address + 6),
            CameraY: ReadWord(bus, address + 8),
            SamusYOffset: ReadWord(bus, address + 10),
            SamusXOffset: ReadWord(bus, address + 12));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
}
