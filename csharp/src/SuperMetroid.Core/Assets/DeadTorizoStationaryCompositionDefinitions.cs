namespace SuperMetroid.Core.Assets;

/// <summary>Tile coverage for one stock sprite in the stationary Dead Torizo composition.</summary>
/// <param name="Tile">OAM tile number locating the part in the source sheet.</param>
/// <param name="TileExtent">Side length of the part in square SNES tiles.</param>
internal readonly record struct DeadTorizoStockPart(int Tile, int TileExtent);

/// <summary>
/// Stock $A9:D6E2-D760 composition, separate from editable display OAM. Its authored
/// head/body/ankle/foot arrangement remains required under Stream3's existing
/// EnemySpritemapCatalog.frames entry, frame dead_torizo_stationary_a9_d6e2.
/// No artwork-retention exception is asserted here. Corpse staging derives from
/// this composition's source-tile coverage instead of duplicating a crop outline.
/// </summary>
internal static class DeadTorizoStationaryCompositionDefinitions
{
    /// <summary>$A9:D6E2 declares25parts: ankle, three head, twenty body and one rear foot.</summary>
    internal const int Count = 25;
    /// <summary>$B7:A800 source sheet is sixteen tiles wide; OAM tile $100 begins that sheet.</summary>
    internal const int SourceColumns = 16, SourceTileBase = 0x100;
    /// <summary>$A9:D6E4: the small rear-ankle fragment uses tile $197 at (-24,20).</summary>
    private const int AnkleTile = 0x197;
    /// <summary>$A9:D6E9-D6F7: head/shoulder row uses tiles $109/$10B/$10D from (-8,-52).</summary>
    private const int HeadTile = 0x109, HeadParts = 3;
    /// <summary>$A9:D6F8-D75B: five rows of four large body parts start at tile $128, origin(-16,-36).</summary>
    private const int BodyTile = 0x128, BodyColumns = 4, BodyRows = 5;
    /// <summary>$A9:D75C-D760: the large rear-foot part uses tile $1A6 at(-32,28).</summary>
    private const int FootTile = 0x1a6;

    /// <summary>Side length, in tiles, of each large head, body, or foot part.</summary>
    private const int LargeExtent = 2;

    /// <summary>Returns one of the 25 stock parts in table order, with its source OAM tile and tile-square extent.</summary>
    /// <param name="index">Zero-based part index: ankle, three head parts, twenty body parts, then rear foot.</param>
    /// <returns>The part's source-sheet tile number and square tile extent.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 25-part composition.</exception>
    internal static DeadTorizoStockPart Part(int index)
    {
        if ((uint)index >= Count) throw new IndexOutOfRangeException();
        if (index == 0) return new(AnkleTile, 1);
        if (index <= HeadParts)
        {
            int column = HeadParts - index;
            return new(HeadTile + column * LargeExtent, LargeExtent);
        }
        int bodyPart = index - 1 - HeadParts;
        if (bodyPart < BodyColumns * BodyRows)
        {
            int row = bodyPart / BodyColumns, column = BodyColumns - 1 - bodyPart % BodyColumns;
            return new(BodyTile + row * LargeExtent * SourceColumns + column * LargeExtent, LargeExtent);
        }
        return new(FootTile, LargeExtent);
    }
}
