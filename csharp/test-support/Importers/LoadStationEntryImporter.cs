using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads native load-station records only while importing or verifying a cartridge.</summary>
internal static class LoadStationEntryImporter
{
    /// <summary>
    /// Reads the native placement record selected by an area's load-station list and slot index.
    /// </summary>
    /// <param name="bus">The cartridge address space containing the load-station pointer table and records.</param>
    /// <param name="areaIndex">The retail area whose station list is being read.</param>
    /// <param name="stationIndex">The zero-based station slot within that area's list.</param>
    /// <returns>The room, door, camera, and Samus placement words stored in the selected record.</returns>
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

    /// <summary>
    /// Reads one little-endian word from the cartridge's fixed bank at the supplied address.
    /// </summary>
    /// <param name="bus">The cartridge address space used for the read.</param>
    /// <param name="address">The mapped address of the first byte.</param>
    /// <returns>The two adjacent bytes combined as an unsigned 16-bit value.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
}
