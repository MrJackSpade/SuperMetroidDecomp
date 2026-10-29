namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// Stock $A9:D6E2-D760 composition, separate from editable display OAM. Its authored
/// head/body/ankle/foot arrangement remains required under Stream3's existing
/// EnemySpritemapCatalog frame dead_torizo_stationary.
/// No artwork-retention exception is asserted here. Corpse staging derives from
/// this composition's source-tile coverage instead of duplicating a crop outline.
/// </summary>
internal static class DeadTorizoStationaryCompositionDefinitions
{
    /// <summary>$A9:D6E2 declares25parts: ankle, three head, twenty body and one rear foot.</summary>
    internal const int Count = 25;
    /// <summary>The $B7:A800 source sheet is sixteen tiles wide; a large part spans two source rows.</summary>
    internal const int SourceColumns = 16;
    /// <summary>OAM tile $100 starts the Dead Torizo source sheet.</summary>
    internal const int SourceTileBase = 0x100;
    /// <summary>$A9:D6E4: the small rear-ankle fragment uses tile $197 at (-24,20).</summary>
    private const int AnkleTile = 0x197;
    /// <summary>$A9:D6E9 begins the head and shoulder row at tile $109.</summary>
    private const int HeadTile = 0x109;
    /// <summary>Three large OAM parts cover the head and shoulder row.</summary>
    private const int HeadParts = 3;
    /// <summary>$A9:D6F8 begins the five-row body grid at tile $128.</summary>
    private const int BodyTile = 0x128;
    /// <summary>Four large parts span each body row in reverse OAM order.</summary>
    private const int BodyColumns = 4;
    /// <summary>Five body rows cover the corpse's torso and legs.</summary>
    private const int BodyRows = 5;
    /// <summary>$A9:D75C-D760: the large rear-foot part uses tile $1A6 at(-32,28).</summary>
    private const int FootTile = 0x1a6;
    /// <summary>A large OAM part covers two source tiles per axis.</summary>
    private const int LargeExtent = 2;

    /// <summary>Maps one native part ordinal to its source tile and small or large tile extent.</summary>
    internal static DeadTorizoStockPart Part(int index)
    {
        if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
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
