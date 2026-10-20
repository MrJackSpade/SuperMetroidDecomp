using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>$8C:A5E2 afterglow: atlas rows $19..1E map to eight-pixel rows
/// around screen Y=0 at atlas row$1B, with X centered at column8. Row$18 packs
/// the four small upper-cap tiles, four large upper-body tiles, then four small
/// lower-cap tiles. The native ordered row runs are calculated; differing supplied
/// tile selections remain independently editable.</summary>
internal sealed class EndingExplosionAfterglowParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Supplied tile selectors when they differ from the 37-part stock atlas composition.</summary>
    private readonly int[]? tiles;
    /// <summary>Number of native cap, wing, and body parts in the complete afterglow frame.</summary>
    private const int StockPartCount = 37;
    /// <summary>Stores edited tile selectors only when the supplied composition differs from stock.</summary>
    private EndingExplosionAfterglowParts(int[] suppliedTiles)
    {
        bool stock = suppliedTiles.Length == StockPartCount;
        for (int index = 0; stock && index < suppliedTiles.Length; index++)
            stock = suppliedTiles[index] == StockTile(index);
        tiles = stock ? null : suppliedTiles;
    }

    /// <summary>Atlas columns of the three lower-wing pieces: left outer, right outer, left inner.</summary>
    private static readonly int[] LowerWingColumns = [4, 10, 5];

    /// <summary>Original $8C:A5E2 draw order through cap, wing and body atlas regions.
    /// Large sprites step two tile columns; small cap/wing sprites step one.</summary>
    internal static int StockTile(int index)
    {
        if ((uint)index >= StockPartCount) throw new ArgumentOutOfRangeException(nameof(index));
        // Lower cap begins at its left end, then consumes the remaining columns backwards.
        if (index < 4) return Atlas(24, index == 0 ? 12 : 16 - index);
        if (index < 7)
            return Atlas(30, LowerWingColumns[index - 4]);
        if (index < 9) return Atlas(29, 8 - 2 * (index - 7)); // Lower body pair.
        if (index < 13) return Atlas(24, 3 - (index - 9)); // Upper cap.
        if (index < 15) return Atlas(27, index == 13 ? 14 : 0); // Right/left outer wings.
        if (index < 19)
        {
            int wingPart = index - 15;
            return Atlas(25, (wingPart < 2 ? 3 : 13) - wingPart % 2);
        }
        if (index < 23) return Atlas(24, 10 - 2 * (index - 19)); // Upper body.
        if (index < 29) return Atlas(28, 12 - 2 * (index - 23)); // Lower belt.
        return Atlas(26, 14 - 2 * (index - 29)); // Full equatorial belt.
    }

    /// <summary>Converts a row and column in the sixteen-column OBJ atlas to a character index.</summary>
    private static int Atlas(int row, int column) => row * 16 + column;

    /// <summary>Recalculates the afterglow only for its native pose and supported atlas region, preserving edited tile choices.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != EndingExplosionSpriteDefinitions.Pointer(EndingExplosionSpriteDefinitions.Pose.Afterglow)) return supplied;
        var tiles = new int[supplied.PartCount];
        for (int index = 0; index < tiles.Length; index++)
        {
            int tile = supplied.Part(index).Attributes.TileNumber;
            if (tile < SnesCharacterGrid.Cell(24, 0) || tile > SnesCharacterGrid.Cell(30, 15)) return supplied;
            tiles[index] = tile;
        }
        return supplied.CalculateIfMatching(new EndingExplosionAfterglowParts(tiles));
    }

    /// <summary>Uses the supplied part count for edited compositions and 37 parts for stock art.</summary>
    public int Count => tiles?.Length ?? StockPartCount;
    /// <summary>Places one cap, wing, or body tile around the screen-centered afterglow origin.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile = tiles is null ? StockTile(index) : tiles[index];
            int row = tile / 16, column = tile % 16;
            int x = (column - 8) * 8, y = (row - 27) * 8;
            bool large = row is >= 26 and <= 29;
            if (row == 24)
            {
                if (column < 4) { x = (column - 2) * 8; y = -32; }
                else if (column >= 12) { x = (column - 14) * 8; y = 32; }
                else { y = -24; large = true; }
            }
            return new(SnesSpritemapXWord.Create(x, large), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(tile, 0, 0), true);
        }
    }
    /// <summary>Enumerates afterglow pieces in native cap, wing, and body draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same part order to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
