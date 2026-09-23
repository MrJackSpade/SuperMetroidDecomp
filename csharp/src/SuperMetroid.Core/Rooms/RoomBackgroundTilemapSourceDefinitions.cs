namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The 58 distinct compressed visual tilemap sources selected by the pinned NTSC
/// Japan/USA v1.0 bank-$8F library-background command lists. These are immutable
/// source identities for artwork import and lookup, not editable command scripts.
/// Keeping the complete set compiled lets installation validation reject a missing
/// or substituted source without reading the cartridge at runtime.
/// </summary>
public static class RoomBackgroundTilemapSources
{
    private static readonly int[] Sources =
    [
        0xb9a634, 0xb9a714, 0xb9a7a8, 0xb9a83a, 0xb9ac83, 0xb9aeff,
        0xb9b2f0, 0xb9b6bb, 0xb9bba5, 0xb9bf3b, 0xb9c26f, 0xb9c5c8,
        0xb9c972, 0xb9cd01, 0xb9ce9f, 0xb9cff8, 0xb9d1fb, 0xb9d38f,
        0xb9d3c5, 0xb9d3fb, 0xb9d5d8, 0xb9d715, 0xb9e1b3, 0xb9e61c,
        0xb9e885, 0xb9ea80, 0xb9ebc7, 0xb9ee52, 0xb9f1c8, 0xb9f94f,
        0xb9fa38, 0xb9fe3e, 0xba807e, 0xba82c4, 0xba8437, 0xba85ba,
        0xba86fc, 0xba8780, 0xba8a49, 0xba8acd, 0xba8dbd, 0xba8de7,
        0xba9386, 0xba988d, 0xba9c35, 0xba9f12, 0xbaa119, 0xbaa475,
        0xbaa69f, 0xbaaa78, 0xbaadf0, 0xbaafe6, 0xbab36b, 0xbab5d8,
        0xbab9a3, 0xbabdd9, 0xbac22a, 0xbac4bc,
    ];

    /// <summary>Sorted, immutable source identities for every retail library BG tilemap.</summary>
    public static IReadOnlyList<int> All { get; } = Array.AsReadOnly(Sources);

    /// <summary>Whether an address identifies one of the pinned visual tilemap streams.</summary>
    public static bool Contains(int sourceAddress) =>
        Array.BinarySearch(Sources, sourceAddress) >= 0;
}
