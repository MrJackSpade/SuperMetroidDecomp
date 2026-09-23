using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Compiled application-owned room, door, camera, and Samus placement records consumed by
/// native <c>LoadFromLoadStation</c> at $80:C437.
/// </summary>
public static class LoadStationDefinitions
{
    /// <summary>The seven native area-list pointers from $80:C4B5, retained as debugger identity.</summary>
    /// <remarks>
    /// Issue #1044: in the pinned NTSC J/U v1.0 ROM, the eight physical
    /// words at $80:C4B5..C4C4 are $C4C5, $C5CF, $C6D9, $C81B,
    /// $C917, $CA2F, $CB2B, and $CC19. The first seven are the typed
    /// retail areas; the eighth starts an untyped debug list after Ceres.
    /// Every station record is 14 bytes, so pointer for area a is $C4C5
    /// plus 14 times the sum of prior retail list lengths (19, 19, 23,
    /// 18, 20, 18, 17). This prefix-sum algorithm reproduces all eight
    /// physical pointers, including the debug boundary after 134 records.
    /// AreaIds.ToIndex restricts this managed view to 0..6, and Get
    /// checks each area's station bound. The independent ROM verifier
    /// matches all 134 retail records and rejects invalid area/station
    /// inputs. Retain these seven named pointers as debugger identities.
    /// </remarks>
    private static readonly ushort[] listPointers =
        [0xc4c5, 0xc5cf, 0xc6d9, 0xc81b, 0xc917, 0xca2f, 0xcb2b];

    /// <summary>The cartridge's deliberately inert placeholder for unused station indexes.</summary>
    private static readonly LoadStationDefinition unused =
        new(0x0000, 0x0000, 0x0000, 0x0400, 0x0400, 0x00b0, 0x0000);

    /// <summary>$80:C4C5-$80:C5CE, all nineteen Crateria records.</summary>
    /// <remarks>
    /// Issue #1047: in the pinned NTSC J/U v1.0 ROM, indexes 2..7 and
    /// 13..15 are the inert placeholder; 0, 1, 8..12, and 16..18 are
    /// authored placements. Index 16 exactly repeats index 0. This
    /// sparse classification and alias are exact, but the other room,
    /// door, camera, and Samus fields are independent scene choices,
    /// so retain their records. Get accepts only station bytes 0..18;
    /// index 19 starts the adjacent Brinstar list at $80:C5CF.
    /// Direct ROM inspection and the independent 134-record load-station
    /// verifier agree on all nineteen rows and the invalid boundary.
    /// </remarks>
    private static readonly LoadStationDefinition[] crateria =
    [
        new(0x91f8, 0x896a, 0x0000, 0x0400, 0x0400, 0x0040, 0x0000),
        new(0x93d5, 0x899a, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        unused, unused, unused, unused, unused, unused,
        new(0x94cc, 0x8aba, 0x0000, 0x0000, 0x0000, 0x00a8, 0x0000),
        new(0x962a, 0x8a42, 0x0000, 0x0000, 0x0000, 0x00a8, 0x0000),
        new(0x97b5, 0x8b86, 0x0000, 0x0000, 0x0000, 0x0088, 0x0000),
        new(0x9938, 0x8c22, 0x0000, 0x0000, 0x0000, 0x0088, 0x0000),
        new(0xa66a, 0x91f2, 0x0000, 0x0000, 0x0100, 0x0098, 0x0000),
        unused, unused, unused,
        new(0x91f8, 0x896a, 0x0000, 0x0400, 0x0400, 0x0040, 0x0000),
        new(0x94fd, 0x8a7e, 0x0000, 0x0000, 0x0400, 0x0095, 0x0000),
        new(0x91f8, 0x88fe, 0x0000, 0x0400, 0x0000, 0x0080, 0x0000),
    ];

    /// <summary>$80:C5CF-$80:C6D8, all nineteen Brinstar records.</summary>
    private static readonly LoadStationDefinition[] brinstar =
    [
        new(0xa184, 0x8df6, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        new(0xa201, 0x8d12, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        new(0xa22a, 0x8f52, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        new(0xa70b, 0x9186, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        new(0xa734, 0x90d2, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        unused, unused, unused,
        new(0x9ad9, 0x8d42, 0x0001, 0x0000, 0x0200, 0x00a8, 0x0000),
        new(0x9e9f, 0x8e86, 0x0000, 0x0500, 0x0200, 0x00a8, 0x0000),
        new(0xa322, 0x908a, 0x0000, 0x0000, 0x0200, 0x00a8, 0x0000),
        new(0xa6a1, 0xa384, 0x0000, 0x0000, 0x0000, 0x0088, 0x0000),
        unused, unused, unused, unused,
        new(0x9ad9, 0x8d42, 0x0001, 0x0000, 0x0200, 0x00a8, 0x0000),
        new(0xa56b, 0x91ce, 0x0000, 0x0000, 0x0100, 0x0080, 0x0000),
        new(0x9d19, 0x8e62, 0x0000, 0x0300, 0x0000, 0x0080, 0x0000),
    ];

    /// <summary>$80:C6D9-$80:C81A, all twenty-three Norfair records.</summary>
    private static readonly LoadStationDefinition[] norfair =
    [
        new(0xaab5, 0x9456, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        new(0xb0dd, 0x959a, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        new(0xb167, 0x97da, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        new(0xb192, 0x93ba, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        new(0xb1bb, 0x9702, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        new(0xb741, 0x9a0e, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        unused, unused,
        new(0xa7de, 0x92a6, 0x0000, 0x0000, 0x0200, 0x00a8, 0x0000),
        new(0xaf3f, 0x96de, 0x0000, 0x0000, 0x0000, 0x0088, 0x0000),
        new(0xb236, 0x9846, 0x0000, 0x0400, 0x0200, 0x0088, 0x0000),
        unused, unused, unused, unused, unused,
        new(0xa7de, 0x932a, 0x0002, 0x0000, 0x0200, 0x00a8, 0x0000),
        new(0xa923, 0x93ea, 0x0001, 0x0c00, 0x0200, 0x00a0, 0x0000),
        new(0xb37a, 0x995a, 0x0000, 0x0000, 0x0000, 0x00a0, 0x0000),
        new(0xaa82, 0x946e, 0x0000, 0x0000, 0x0000, 0x00b5, 0x0000),
        new(0xb236, 0x9846, 0x0001, 0x0500, 0x0200, 0x0035, 0x0000),
        new(0xb283, 0x98a6, 0x0000, 0x0200, 0x0200, 0x0000, 0x0000),
        new(0xb283, 0x983a, 0x0000, 0x0000, 0x0000, 0x0080, 0x0000),
    ];

    /// <summary>$80:C81B-$80:C916, all eighteen Wrecked Ship records.</summary>
    /// <remarks>
    /// Issue #1046: the pinned NTSC J/U v1.0 ROM uses the inert placeholder
    /// (0,0,0,$0400,$0400,$00B0,0) at station indexes 1..15. Only 0,
    /// 16, and 17 contain distinct authored room, door, camera, and Samus
    /// placement records. That sparse index classification is exact, but
    /// those three destinations are scene identities with no useful
    /// deterministic formula; retain the named records and placeholder.
    /// Get bounds the station byte to 0..17. Index 18 begins the adjacent
    /// Maridia list at $80:C917 and must not be treated as Wrecked Ship.
    /// Direct ROM inspection and the independent 134-record load-station
    /// oracle agree on all eighteen values and the invalid boundary.
    /// </remarks>
    private static readonly LoadStationDefinition[] wreckedShip =
    [
        new(0xce8a, 0xa240, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        unused, unused, unused, unused, unused, unused, unused,
        unused, unused, unused, unused, unused, unused, unused, unused,
        new(0xca08, 0xa1f8, 0x0001, 0x0000, 0x0000, 0x0080, 0x0000),
        new(0xcc6f, 0xa2b8, 0x0000, 0x0400, 0x0000, 0x0080, 0x0000),
    ];

    /// <summary>$80:C917-$80:CA2E, all twenty Maridia records.</summary>
    private static readonly LoadStationDefinition[] maridia =
    [
        new(0xced2, 0xa354, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        new(0xd3df, 0xa588, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        new(0xd765, 0xa744, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        new(0xd81a, 0xa7ec, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        unused, unused, unused, unused,
        new(0xd30b, 0xa570, 0x0000, 0x0000, 0x0200, 0x00a8, 0x0000),
        unused, unused, unused, unused, unused, unused, unused,
        new(0xd1dd, 0xa4a4, 0x0001, 0x0000, 0x0000, 0x00d0, 0x0000),
        new(0xd78f, 0xa81c, 0x0000, 0x0000, 0x0200, 0x0080, 0x0000),
        new(0xd617, 0xa72c, 0x0000, 0x0300, 0x0000, 0x0080, 0x0000),
        new(0xd48e, 0xa648, 0x0000, 0x0000, 0x0100, 0x0080, 0x0000),
    ];

    /// <summary>$80:CA2F-$80:CB2A, all eighteen Tourian records.</summary>
    private static readonly LoadStationDefinition[] tourian =
    [
        new(0xde23, 0xaabc, 0x0000, 0x0000, 0x0000, 0x0098, 0xffe0),
        new(0xdf1b, 0xa99c, 0x0000, 0x0000, 0x0000, 0x0098, 0x0000),
        unused, unused, unused, unused, unused, unused,
        new(0xdaae, 0xa9a8, 0x0000, 0x0000, 0x0200, 0x00a8, 0x0000),
        unused, unused, unused, unused, unused, unused, unused,
        new(0xddf3, 0xaaa4, 0x0000, 0x0000, 0x0200, 0x0080, 0x0000),
        new(0xddf3, 0xaa38, 0x0000, 0x0000, 0x0000, 0x0080, 0x0000),
    ];

    /// <summary>$80:CB2B-$80:CC18, all seventeen Ceres records.</summary>
    /// <remarks>
    /// Issue #1045: all 17 fourteen-byte records in the pinned NTSC J/U
    /// v1.0 ROM have room $DF45, door $AB58, and zero BTS, camera, and
    /// Samus-X fields. Samus-Y offset is $0048 at station 0 and $0040
    /// at stations 1..16. This constant record with an index-zero
    /// exception is the exact bounded algorithm for Ceres station byte
    /// 0..16; the repeated placements remain authored scene state.
    /// Station 17 would start adjacent debug data at $80:CC19, so Get
    /// rejects it instead of extending the pattern. Direct ROM inspection
    /// checked all 17 rows; the independent load-station verifier matches
    /// them and the other 117 retail records and checks the boundary.
    /// </remarks>
    private static readonly LoadStationDefinition[] ceres =
    [
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0048, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
        new(0xdf45, 0xab58, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000),
    ];

    /// <summary>Returns the exact number of defined slots, including native placeholders.</summary>
    public static int Count(AreaId area) => GetArea(area).Length;

    /// <summary>Returns one compiled record without consulting the cartridge address space.</summary>
    public static LoadStationEntry Get(AreaId areaIndex, byte stationIndex)
    {
        int area = AreaIds.ToIndex(areaIndex);
        ReadOnlySpan<LoadStationDefinition> entries = GetArea(areaIndex);
        if (stationIndex >= entries.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(stationIndex), stationIndex,
                $"{areaIndex} has {entries.Length} load-station slots.");
        }

        LoadStationDefinition entry = entries[stationIndex];
        return new LoadStationEntry(areaIndex, stationIndex, listPointers[area],
            entry.RoomPointer, entry.DoorPointer, entry.DoorBts, entry.CameraX,
            entry.CameraY, entry.SamusYOffset, entry.SamusXOffset);
    }

    private static ReadOnlySpan<LoadStationDefinition> GetArea(AreaId area) => area switch
    {
        AreaId.Crateria => crateria,
        AreaId.Brinstar => brinstar,
        AreaId.Norfair => norfair,
        AreaId.WreckedShip => wreckedShip,
        AreaId.Maridia => maridia,
        AreaId.Tourian => tourian,
        AreaId.Ceres => ceres,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown retail area ID."),
    };

    private readonly record struct LoadStationDefinition(
        ushort RoomPointer,
        ushort DoorPointer,
        ushort DoorBts,
        ushort CameraX,
        ushort CameraY,
        ushort SamusYOffset,
        ushort SamusXOffset);
}
