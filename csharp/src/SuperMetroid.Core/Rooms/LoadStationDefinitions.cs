using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Direct load-station placement selection for native LoadFromLoadStation ($80:C437).</summary>
/// <remarks>Area lists are consecutive fourteen-byte records starting at $80:C4C5.
/// Named area/station cases select active room, door, BTS and placement settings;
/// valid unused slots select the native inert placeholder. Ceres repeats one
/// placement with Y offset72 for station0 and64 for stations1..16. Debug-area data
/// beginning atCC19 is outside the accepted domain. No record or pointer array is stored.</remarks>
public static class LoadStationDefinitions
{
    /// <summary>Native list lengths, including inert placeholders.</summary>
    public static int Count(AreaId area) => area switch
    {
        AreaId.Crateria => 19,
        AreaId.Brinstar => 19,
        AreaId.Norfair => 23,
        AreaId.WreckedShip => 18,
        AreaId.Maridia => 20,
        AreaId.Tourian => 18,
        AreaId.Ceres => 17,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown retail area ID."),
    };

    /// <summary>Constructs one exact native placement after validating area and station bounds.</summary>
    public static LoadStationEntry Get(AreaId areaIndex, byte stationIndex)
    {
        int area = AreaIds.ToIndex(areaIndex);
        int count = Count(areaIndex);
        if (stationIndex >= count)
            throw new ArgumentOutOfRangeException(nameof(stationIndex), stationIndex,
                $"{areaIndex} has {count} load-station slots.");
        int preceding = 0;
        for (int index = 0; index < area; index++) preceding += Count((AreaId)index);
        ushort listPointer = checked((ushort)((LoadStationRomData.DataStart & 0xffff) +
            LoadStationRomData.EntryByteCount * preceding));
        LoadStationDefinition entry = areaIndex == AreaId.Ceres
            ? new(0xdf45, 0xab58, 0, 0, 0, (ushort)(stationIndex == 0 ? 72 : 64), 0)
            : Placement(areaIndex, stationIndex);
        return new LoadStationEntry(areaIndex, stationIndex, listPointer,
            entry.RoomPointer, entry.DoorPointer, entry.DoorBts, entry.CameraX,
            entry.CameraY, entry.SamusYOffset, entry.SamusXOffset);
    }

    // Only validated non-Ceres slots reach this selector. The default is the
    // native unused-station record, not an extension to unknown area/station inputs.
    private static LoadStationDefinition Placement(AreaId area, byte station) => (area, station) switch
    {
        (AreaId.Crateria, 0) => new(0x91f8, 0x896a, 0x0000, 0x0400, 0x0400, 0x0040, 0x0000),
        (AreaId.Crateria, 1) => new(0x93d5, 0x899a, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        (AreaId.Crateria, 8) => new(0x94cc, 0x8aba, 0x0000, 0x0000, 0x0000, 0x00a8, 0x0000),
        (AreaId.Crateria, 9) => new(0x962a, 0x8a42, 0x0000, 0x0000, 0x0000, 0x00a8, 0x0000),
        (AreaId.Crateria, 10) => new(0x97b5, 0x8b86, 0x0000, 0x0000, 0x0000, 0x0088, 0x0000),
        (AreaId.Crateria, 11) => new(0x9938, 0x8c22, 0x0000, 0x0000, 0x0000, 0x0088, 0x0000),
        (AreaId.Crateria, 12) => new(0xa66a, 0x91f2, 0x0000, 0x0000, 0x0100, 0x0098, 0x0000),
        (AreaId.Crateria, 16) => new(0x91f8, 0x896a, 0x0000, 0x0400, 0x0400, 0x0040, 0x0000),
        (AreaId.Crateria, 17) => new(0x94fd, 0x8a7e, 0x0000, 0x0000, 0x0400, 0x0095, 0x0000),
        (AreaId.Crateria, 18) => new(0x91f8, 0x88fe, 0x0000, 0x0400, 0x0000, 0x0080, 0x0000),
        (AreaId.Brinstar, 0) => new(0xa184, 0x8df6, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        (AreaId.Brinstar, 1) => new(0xa201, 0x8d12, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        (AreaId.Brinstar, 2) => new(0xa22a, 0x8f52, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        (AreaId.Brinstar, 3) => new(0xa70b, 0x9186, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Brinstar, 4) => new(0xa734, 0x90d2, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Brinstar, 8) => new(0x9ad9, 0x8d42, 0x0001, 0x0000, 0x0200, 0x00a8, 0x0000),
        (AreaId.Brinstar, 9) => new(0x9e9f, 0x8e86, 0x0000, 0x0500, 0x0200, 0x00a8, 0x0000),
        (AreaId.Brinstar, 10) => new(0xa322, 0x908a, 0x0000, 0x0000, 0x0200, 0x00a8, 0x0000),
        (AreaId.Brinstar, 11) => new(0xa6a1, 0xa384, 0x0000, 0x0000, 0x0000, 0x0088, 0x0000),
        (AreaId.Brinstar, 16) => new(0x9ad9, 0x8d42, 0x0001, 0x0000, 0x0200, 0x00a8, 0x0000),
        (AreaId.Brinstar, 17) => new(0xa56b, 0x91ce, 0x0000, 0x0000, 0x0100, 0x0080, 0x0000),
        (AreaId.Brinstar, 18) => new(0x9d19, 0x8e62, 0x0000, 0x0300, 0x0000, 0x0080, 0x0000),
        (AreaId.Norfair, 0) => new(0xaab5, 0x9456, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Norfair, 1) => new(0xb0dd, 0x959a, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        (AreaId.Norfair, 2) => new(0xb167, 0x97da, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Norfair, 3) => new(0xb192, 0x93ba, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Norfair, 4) => new(0xb1bb, 0x9702, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        (AreaId.Norfair, 5) => new(0xb741, 0x9a0e, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Norfair, 8) => new(0xa7de, 0x92a6, 0x0000, 0x0000, 0x0200, 0x00a8, 0x0000),
        (AreaId.Norfair, 9) => new(0xaf3f, 0x96de, 0x0000, 0x0000, 0x0000, 0x0088, 0x0000),
        (AreaId.Norfair, 10) => new(0xb236, 0x9846, 0x0000, 0x0400, 0x0200, 0x0088, 0x0000),
        (AreaId.Norfair, 16) => new(0xa7de, 0x932a, 0x0002, 0x0000, 0x0200, 0x00a8, 0x0000),
        (AreaId.Norfair, 17) => new(0xa923, 0x93ea, 0x0001, 0x0c00, 0x0200, 0x00a0, 0x0000),
        (AreaId.Norfair, 18) => new(0xb37a, 0x995a, 0x0000, 0x0000, 0x0000, 0x00a0, 0x0000),
        (AreaId.Norfair, 19) => new(0xaa82, 0x946e, 0x0000, 0x0000, 0x0000, 0x00b5, 0x0000),
        (AreaId.Norfair, 20) => new(0xb236, 0x9846, 0x0001, 0x0500, 0x0200, 0x0035, 0x0000),
        (AreaId.Norfair, 21) => new(0xb283, 0x98a6, 0x0000, 0x0200, 0x0200, 0x0000, 0x0000),
        (AreaId.Norfair, 22) => new(0xb283, 0x983a, 0x0000, 0x0000, 0x0000, 0x0080, 0x0000),
        (AreaId.WreckedShip, 0) => new(0xce8a, 0xa240, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.WreckedShip, 16) => new(0xca08, 0xa1f8, 0x0001, 0x0000, 0x0000, 0x0080, 0x0000),
        (AreaId.WreckedShip, 17) => new(0xcc6f, 0xa2b8, 0x0000, 0x0400, 0x0000, 0x0080, 0x0000),
        (AreaId.Maridia, 0) => new(0xced2, 0xa354, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Maridia, 1) => new(0xd3df, 0xa588, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Maridia, 2) => new(0xd765, 0xa744, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        (AreaId.Maridia, 3) => new(0xd81a, 0xa7ec, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Maridia, 8) => new(0xd30b, 0xa570, 0x0000, 0x0000, 0x0200, 0x00a8, 0x0000),
        (AreaId.Maridia, 16) => new(0xd1dd, 0xa4a4, 0x0001, 0x0000, 0x0000, 0x00d0, 0x0000),
        (AreaId.Maridia, 17) => new(0xd78f, 0xa81c, 0x0000, 0x0000, 0x0200, 0x0080, 0x0000),
        (AreaId.Maridia, 18) => new(0xd617, 0xa72c, 0x0000, 0x0300, 0x0000, 0x0080, 0x0000),
        (AreaId.Maridia, 19) => new(0xd48e, 0xa648, 0x0000, 0x0000, 0x0100, 0x0080, 0x0000),
        (AreaId.Tourian, 0) => new(0xde23, 0xaabc, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        (AreaId.Tourian, 1) => new(0xdf1b, 0xa99c, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        (AreaId.Tourian, 8) => new(0xdaae, 0xa9a8, 0x0000, 0x0000, 0x0200, 0x00a8, 0x0000),
        (AreaId.Tourian, 16) => new(0xddf3, 0xaaa4, 0x0000, 0x0000, 0x0200, 0x0080, 0x0000),
        (AreaId.Tourian, 17) => new(0xddf3, 0xaa38, 0x0000, 0x0000, 0x0000, 0x0080, 0x0000),
        _ => new(0, 0, 0, 0x0400, 0x0400, 0x00b0, 0),
    };

    private readonly record struct LoadStationDefinition(
        ushort RoomPointer, ushort DoorPointer, ushort DoorBts, ushort CameraX,
        ushort CameraY, ushort SamusYOffset, ushort SamusXOffset);
}
