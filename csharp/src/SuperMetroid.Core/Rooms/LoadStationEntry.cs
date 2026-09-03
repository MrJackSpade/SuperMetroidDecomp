using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Rooms;

/// <summary>One fourteen-byte entry consumed by <c>LoadFromLoadStation</c> at $80:C437.</summary>
public sealed record LoadStationEntry(
    AreaId RequestedAreaIndex,
    byte StationIndex,
    ushort ListPointer,
    ushort RoomPointer,
    ushort DoorPointer,
    ushort DoorBts,
    ushort CameraX,
    ushort CameraY,
    ushort SamusYOffset,
    ushort SamusXOffset)
{
    private const int LoadStationPointerTable = 0x80c4b5;
    private const int EntryByteCount = 14;

    /// <summary>Reads the area list pointer and indexed record with bank-$80 wrapping.</summary>
    public static LoadStationEntry Load(ISnesAddressSpace bus, AreaId areaIndex, byte stationIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int areaTableIndex = AreaIds.ToIndex(areaIndex);
        ushort listPointer = RomDataReader.ReadWordFixedBank(
            bus,
            LoadStationPointerTable + areaTableIndex * 2);
        int address = 0x800000 | unchecked((ushort)(listPointer + stationIndex * EntryByteCount));
        return new LoadStationEntry(
            areaIndex,
            stationIndex,
            listPointer,
            RoomPointer: ReadWord(bus, address),
            DoorPointer: ReadWord(bus, address + 2),
            DoorBts: ReadWord(bus, address + 4),
            CameraX: ReadWord(bus, address + 6),
            CameraY: ReadWord(bus, address + 8),
            SamusYOffset: ReadWord(bus, address + 10),
            SamusXOffset: ReadWord(bus, address + 12));
    }

    /// <summary>World X assigned by the native load-station routine.</summary>
    public ushort SamusX => unchecked((ushort)(CameraX + 128 + SamusXOffset));

    /// <summary>World Y assigned by the native load-station routine.</summary>
    public ushort SamusY => unchecked((ushort)(CameraY + SamusYOffset));

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(bus, address);
}
