using SuperMetroid.Core;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads native load-station records only while importing or verifying a cartridge.</summary>
[CpuBusBoundary("codec: ROM bank-$80 load-station pointer table and entries → LoadStationEntry")]
internal static class LoadStationEntryImporter
{
    /// <summary>Load.</summary>
    public static LoadStationEntry Load(ISnesAddressSpace bus, AreaId areaIndex, byte stationIndex)
    {
        Ensure.NotNull(bus);
        int areaTableIndex = AreaIds.ToIndex(areaIndex);
        ushort listPointer = RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus),
            LoadStationRomData.PointerTable + areaTableIndex * 2);
        int address = 0x800000 | unchecked((ushort)(listPointer + stationIndex * LoadStationRomData.EntryByteCount));
        return new LoadStationEntry(
            RoomPointer: ReadWord(bus, address),
            DoorPointer: ReadWord(bus, address + 2),
            CameraPosition: new(ReadWord(bus, address + 6), ReadWord(bus, address + 8)),
            SamusOffset: new(unchecked((short)ReadWord(bus, address + 12)), unchecked((short)ReadWord(bus, address + 10))));
    }

    /// <summary>Read word.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
}
