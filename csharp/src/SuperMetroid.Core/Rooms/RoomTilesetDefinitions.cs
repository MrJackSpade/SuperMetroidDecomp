namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The complete twenty-nine-entry $8F:E7A7 retail graphics-set table and its nine-byte
/// definitions. These pointers identify cartridge input assets; they do not script uploads,
/// scrolling, collision, or any other mutable room behavior.
/// </summary>
public static class RoomTilesetDefinitions
{
    /// <summary>Graphics sets $00-$1C are the complete authored table before unrelated bank-$8F data.</summary>
    public const int Count = 0x1d;

    private static readonly TilesetDefinition[] Entries =
    [
        new(0xe6a2, 0xc1b6f6, 0xbac629, 0xc2ad7c),
        new(0xe6ab, 0xc1b6f6, 0xbac629, 0xc2ae5d),
        new(0xe6b4, 0xc1beee, 0xbaf911, 0xc2af43),
        new(0xe6bd, 0xc1beee, 0xbaf911, 0xc2b015),
        new(0xe6c6, 0xc1c5cf, 0xbbae9e, 0xc2b0e7),
        new(0xe6cf, 0xc1c5cf, 0xbbae9e, 0xc2b1a6),
        new(0xe6d8, 0xc1cfa6, 0xbbe6b0, 0xc2b264),
        new(0xe6e1, 0xc1d8dc, 0xbca5aa, 0xc2b35f),
        new(0xe6ea, 0xc1d8dc, 0xbca5aa, 0xc2b447),
        new(0xe6f3, 0xc1e361, 0xbdc3f9, 0xc2b5e4),
        new(0xe6fc, 0xc1e361, 0xbdc3f9, 0xc2b6bb),
        new(0xe705, 0xc1f4b1, 0xbeb130, 0xc2b83c),
        new(0xe70e, 0xc2855f, 0xbee78d, 0xc2b92e),
        new(0xe717, 0xc29b01, 0xbfd414, 0xc2baed),
        new(0xe720, 0xc29b01, 0xbfd414, 0xc2bbc1),
        new(0xe729, 0xc2a75e, 0xc0b004, 0xc2c104),
        new(0xe732, 0xc2a75e, 0xc0b004, 0xc2c1e3),
        new(0xe73b, 0xc2a75e, 0xc0e22a, 0xc2c104),
        new(0xe744, 0xc2a75e, 0xc0e22a, 0xc2c1e3),
        new(0xe74d, 0xc2a75e, 0xc18da9, 0xc2c104),
        new(0xe756, 0xc2a75e, 0xc18da9, 0xc2c1e3),
        new(0xe75f, 0xc2a27b, 0xc0860b, 0xc2bc9c),
        new(0xe768, 0xc2a27b, 0xc0860b, 0xc2bd7b),
        new(0xe771, 0xc2a27b, 0xc0860b, 0xc2be58),
        new(0xe77a, 0xc2a27b, 0xc0860b, 0xc2bf3d),
        new(0xe783, 0xc2a27b, 0xc0860b, 0xc2c021),
        new(0xe78c, 0xc1e189, 0xbcdff0, 0xc2b510),
        new(0xe795, 0xc1f3af, 0xbdfe2a, 0xc2b798),
        new(0xe79e, 0xc2960d, 0xbf9dea, 0xc2ba2c),
    ];

    /// <summary>Returns one immutable retail definition by the room state's graphics-set index.</summary>
    public static TilesetDefinition Get(byte graphicsSet)
    {
        if (graphicsSet >= Entries.Length)
            throw new InvalidDataException($"Graphics set ${graphicsSet:X2} is outside the compiled retail tileset table.");
        return Entries[graphicsSet];
    }
}
