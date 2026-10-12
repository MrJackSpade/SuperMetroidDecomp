using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

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
            ? new(0xdf45, 0xab58, Position16<RoomPixels>.Zero, new(0, (short)(stationIndex == 0 ? 72 : 64)))
            : Placement(areaIndex, stationIndex);
        return new LoadStationEntry(entry.RoomPointer, entry.DoorPointer, entry.CameraPosition, entry.SamusOffset);
    }

    // Only validated non-Ceres slots reach this selector. The default is the
    // native unused-station record, not an extension to unknown area/station inputs.
    private static LoadStationDefinition Placement(AreaId area, byte station) => (area, station) switch
    {
        (AreaId.Crateria, 0) => new(0x91f8, 0x896a, new(0x0400, 0x0400), new(0x0000, 0x0040)),
        (AreaId.Crateria, 1) => new(0x93d5, 0x899a, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Crateria, 8) => new(0x94cc, 0x8aba, new(0x0000, 0x0000), new(0x0000, 0x00a8)),
        (AreaId.Crateria, 9) => new(0x962a, 0x8a42, new(0x0000, 0x0000), new(0x0000, 0x00a8)),
        (AreaId.Crateria, 10) => new(0x97b5, 0x8b86, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Crateria, 11) => new(0x9938, 0x8c22, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Crateria, 12) => new(0xa66a, 0x91f2, new(0x0000, 0x0100), new(0x0000, 0x0098)),
        (AreaId.Crateria, 16) => new(0x91f8, 0x896a, new(0x0400, 0x0400), new(0x0000, 0x0040)),
        (AreaId.Crateria, 17) => new(0x94fd, 0x8a7e, new(0x0000, 0x0400), new(0x0000, 0x0095)),
        (AreaId.Crateria, 18) => new(0x91f8, 0x88fe, new(0x0400, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Brinstar, 0) => new(0xa184, 0x8df6, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 1) => new(0xa201, 0x8d12, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 2) => new(0xa22a, 0x8f52, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 3) => new(0xa70b, 0x9186, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Brinstar, 4) => new(0xa734, 0x90d2, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Brinstar, 8) => new(0x9ad9, 0x8d42, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 9) => new(0x9e9f, 0x8e86, new(0x0500, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 10) => new(0xa322, 0x908a, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 11) => new(0xa6a1, 0xa384, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Brinstar, 16) => new(0x9ad9, 0x8d42, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 17) => new(0xa56b, 0x91ce, new(0x0000, 0x0100), new(0x0000, 0x0080)),
        (AreaId.Brinstar, 18) => new(0x9d19, 0x8e62, new(0x0300, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Norfair, 0) => new(0xaab5, 0x9456, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 1) => new(0xb0dd, 0x959a, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Norfair, 2) => new(0xb167, 0x97da, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 3) => new(0xb192, 0x93ba, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 4) => new(0xb1bb, 0x9702, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Norfair, 5) => new(0xb741, 0x9a0e, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 8) => new(0xa7de, 0x92a6, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Norfair, 9) => new(0xaf3f, 0x96de, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Norfair, 10) => new(0xb236, 0x9846, new(0x0400, 0x0200), new(0x0000, 0x0088)),
        (AreaId.Norfair, 16) => new(0xa7de, 0x932a, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Norfair, 17) => new(0xa923, 0x93ea, new(0x0c00, 0x0200), new(0x0000, 0x00a0)),
        (AreaId.Norfair, 18) => new(0xb37a, 0x995a, new(0x0000, 0x0000), new(0x0000, 0x00a0)),
        (AreaId.Norfair, 19) => new(0xaa82, 0x946e, new(0x0000, 0x0000), new(0x0000, 0x00b5)),
        (AreaId.Norfair, 20) => new(0xb236, 0x9846, new(0x0500, 0x0200), new(0x0000, 0x0035)),
        (AreaId.Norfair, 21) => new(0xb283, 0x98a6, new(0x0200, 0x0200), new(0x0000, 0x0000)),
        (AreaId.Norfair, 22) => new(0xb283, 0x983a, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        (AreaId.WreckedShip, 0) => new(0xce8a, 0xa240, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.WreckedShip, 16) => new(0xca08, 0xa1f8, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        (AreaId.WreckedShip, 17) => new(0xcc6f, 0xa2b8, new(0x0400, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Maridia, 0) => new(0xced2, 0xa354, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 1) => new(0xd3df, 0xa588, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 2) => new(0xd765, 0xa744, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Maridia, 3) => new(0xd81a, 0xa7ec, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 8) => new(0xd30b, 0xa570, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Maridia, 16) => new(0xd1dd, 0xa4a4, new(0x0000, 0x0000), new(0x0000, 0x00d0)),
        (AreaId.Maridia, 17) => new(0xd78f, 0xa81c, new(0x0000, 0x0200), new(0x0000, 0x0080)),
        (AreaId.Maridia, 18) => new(0xd617, 0xa72c, new(0x0300, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Maridia, 19) => new(0xd48e, 0xa648, new(0x0000, 0x0100), new(0x0000, 0x0080)),
        (AreaId.Tourian, 0) => new(0xde23, 0xaabc, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Tourian, 1) => new(0xdf1b, 0xa99c, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Tourian, 8) => new(0xdaae, 0xa9a8, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Tourian, 16) => new(0xddf3, 0xaaa4, new(0x0000, 0x0200), new(0x0000, 0x0080)),
        (AreaId.Tourian, 17) => new(0xddf3, 0xaa38, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        _ => new(0, 0, new(0x0400, 0x0400), new(0, 0x00b0)),
    };

    private readonly record struct LoadStationDefinition(
        ushort RoomPointer, ushort DoorPointer, Position16<RoomPixels> CameraPosition,
        Offset16<RoomPixels> SamusOffset);
}
