using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A5E2 afterglow: atlas rows $19..1E map to eight-pixel rows
/// around screen Y=0 at atlas row$1B, with X centered at column8. Row$18 packs
/// the four small upper-cap tiles, four large upper-body tiles, then four small
/// lower-cap tiles. The native ordered row runs are calculated; differing supplied
/// tile selections remain independently editable.</summary>
internal sealed class EndingExplosionAfterglowParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Custom tile selections, or <see langword="null"/> when the native 37-part sequence is used.</summary>
    private readonly int[]? tiles;
    /// <summary>Number of compiled sprite parts in the stock afterglow layout.</summary>
    private const int StockPartCount = 37;

    /// <summary>Retains non-stock selections and uses calculated stock tiles when every supplied slot matches.</summary>
    /// <param name="suppliedTiles">Atlas tile numbers in the sprite composition's part order.</param>
    private EndingExplosionAfterglowParts(int[] suppliedTiles)
    {
        bool stock = suppliedTiles.Length == StockPartCount;
        for (int index = 0; stock && index < suppliedTiles.Length; index++)
            stock = suppliedTiles[index] == StockTile(index);
        tiles = stock ? null : suppliedTiles;
    }

    /// <summary>Lower-wing positions in the native three-part selection sequence.</summary>
    private enum LowerWing
    {
        /// <summary>Outermost wing tile on the left side.</summary>
        LeftOuter,
        /// <summary>Outermost wing tile on the right side.</summary>
        RightOuter,
        /// <summary>Inner wing tile on the left side.</summary>
        LeftInner
    }

    /// <summary>Original $8C:A5E2 draw order through cap, wing and body atlas regions.
    /// Large sprites step two tile columns; small cap/wing sprites step one.</summary>
    internal static int StockTile(int index)
    {
        if ((uint)index >= StockPartCount) throw new ArgumentOutOfRangeException(nameof(index));
        // Lower cap begins at its left end, then consumes the remaining columns backwards.
        if (index < 4) return Atlas(0x18, index == 0 ? 12 : 16 - index);
        if (index < 7)
            return Atlas(0x1e, (LowerWing)(index - 4) switch
            {
                LowerWing.LeftOuter => 4,
                LowerWing.RightOuter => 10,
                LowerWing.LeftInner => 5,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            });
        if (index < 9) return Atlas(0x1d, 8 - 2 * (index - 7)); // Lower body pair.
        if (index < 13) return Atlas(0x18, 3 - (index - 9)); // Upper cap.
        if (index < 15) return Atlas(0x1b, index == 13 ? 14 : 0); // Right/left outer wings.
        if (index < 19)
        {
            int wingPart = index - 15;
            return Atlas(0x19, (wingPart < 2 ? 3 : 13) - wingPart % 2);
        }
        if (index < 23) return Atlas(0x18, 10 - 2 * (index - 19)); // Upper body.
        if (index < 29) return Atlas(0x1c, 12 - 2 * (index - 23)); // Lower belt.
        return Atlas(0x1a, 14 - 2 * (index - 29)); // Full equatorial belt.
    }

    /// <summary>Converts a 16-column atlas coordinate into the tile number used by a spritemap.</summary>
    /// <param name="row">Zero-based tile row in the extracted atlas.</param>
    /// <param name="column">Zero-based tile column within that row.</param>
    /// <returns>The row-major atlas tile number.</returns>
    private static int Atlas(int row, int column) => row * 16 + column;

    /// <summary>Recalculates afterglow placement when the composition uses this pose and its tiles belong to the atlas.</summary>
    /// <param name="pointer">Sprite-definition pointer identifying the requested pose.</param>
    /// <param name="supplied">Composition whose selected tiles and current parts are retained.</param>
    /// <returns>The composition recalculated with afterglow placement when eligible; otherwise the supplied composition.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != EndingExplosionSpriteDefinitions.Pointer(EndingExplosionSpriteDefinitions.Pose.Afterglow)) return supplied;
        var tiles = new int[supplied.PartCount];
        for (int index = 0; index < tiles.Length; index++)
        {
            int tile = supplied.Part(index).Attributes.TileNumber;
            if (tile is < 0x180 or > 0x1ef) return supplied;
            tiles[index] = tile;
        }
        return supplied.CalculateIfMatching(new EndingExplosionAfterglowParts(tiles));
    }

    /// <summary>Number of supplied parts, or the full stock afterglow part count.</summary>
    public int Count => tiles?.Length ?? StockPartCount;

    /// <summary>Gets a part with its tile selected from the stock sequence or caller-supplied atlas choices.</summary>
    /// <param name="index">Zero-based position in this afterglow composition.</param>
    /// <returns>The compiled sprite part positioned from its selected atlas row and column.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the composition.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile = tiles is null ? StockTile(index) : tiles[index];
            int row = tile / 16, column = tile % 16;
            int x = (column - 8) * 8, y = (row - 0x1b) * 8;
            bool large = row is >= 0x1a and <= 0x1d;
            if (row == 0x18)
            {
                if (column < 4) { x = (column - 2) * 8; y = -32; }
                else if (column >= 12) { x = (column - 14) * 8; y = 32; }
                else { y = -24; large = true; }
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 0), true);
        }
    }
    /// <summary>Enumerates positioned afterglow parts in native draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
