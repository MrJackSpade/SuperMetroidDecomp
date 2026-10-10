using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>All forty-one parts of8C:9150 from packed body strips, edge patches
/// and their adjoining cells. Preserve the native publication order.</summary>
internal sealed class CeresStationParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Adds the compiled station-under-attack parts only when the sprite pointer selects that spritemap.</summary>
    /// <param name="pointer">Native spritemap pointer being matched against the station-under-attack definition.</param>
    /// <param name="supplied">Composition accumulated from the caller's other matching parts.</param>
    /// <returns>The supplied composition with the station parts included on a pointer match, or unchanged otherwise.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied) =>
        pointer == CeresFlightSpriteDefinitions.StationUnderAttack
            ? supplied.CalculateIfMatching(new CeresStationParts()) : supplied;

    /// <summary>Gets the fixed number of ordered parts in the station-under-attack spritemap.</summary>
    public int Count => 41;

    /// <summary>Gets the compiled spritemap part at its native publication-order index.</summary>
    /// <param name="index">Zero-based part index from 0 through <see cref="Count"/> minus one.</param>
    /// <returns>The tile, position, and spritemap attributes for that part.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the published part range.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            if (index < 2)
            {
                // Overlapping top strip extension, then its lower-left small cell.
                x = -10 - 8 - (index == 0 ? 8 : 0); y = -48 + (index == 0 ? 8 : 0);
                tile = CeresStationAtlas.Top - 1 + (index == 0 ? 15 : 0);
            }
            else if (index == 2)
            {
                x = -50 + 8 * 9; y = 16; tile = CeresStationAtlas.Lower + 9;
            }
            else if (index == 3)
            {
                // Small continuation after the six-part middle strip.
                x = -50 + 6 * 16; y = 0; tile = CeresStationAtlas.RightTip;
            }
            else if (index < 6)
            {
                int column = 5 - index;
                x = -50 + 8 * (8 + column); y = 32;
                tile = CeresStationAtlas.LowerRightEdge + column;
            }
            else if (index == 6)
            {
                // Downward continuation of the left cell of the lower-right pair.
                x = -50 + 6 * 8; y = 32 + 8;
                tile = CeresStationAtlas.LowerRightEdge + 2;
            }
            else if (index < 9)
            {
                int column = 8 - index;
                x = -50 + 8 * (6 + column); y = 32;
                tile = CeresStationAtlas.LowerRightPair + column;
            }
            else if (index < 13)
            {
                // Three-cell left edge plus a downward continuation of its right cell.
                int column = index == 9 ? 2 : 12 - index;
                int row = index == 9 ? 1 : 0;
                x = -50 + 8 * (1 + column); y = 32 + 8 * row;
                tile = CeresStationAtlas.LowerLeftEdge + column + 16 * row;
            }
            else if (index == 13)
            {
                // The large center patch immediately follows the three small edge cells.
                x = -50 + 4 * 8; y = 32; tile = CeresStationAtlas.LowerLeftEdge + 3;
            }
            else if (index < 19)
            {
                int column = 2 * (18 - index);
                x = -50 + 8 * column; y = 16; tile = CeresStationAtlas.Lower + column;
            }
            else if (index < 25)
            {
                int column = 2 * (24 - index);
                x = -50 + 8 * column; y = 0; tile = CeresStationAtlas.Middle + column;
            }
            else if (index < 32)
            {
                int piece = 31 - index;
                int column = 2 * piece - (piece >= 4 ? 1 : 0);
                x = -50 + 8 * column; y = -16;
                tile = CeresStationAtlas.TwoRowStrip(CeresStationAtlas.UpperMiddle, column);
            }
            else if (index < 38)
            {
                int column = Math.Min(2 * (37 - index), 9);
                x = -42 + 8 * column; y = -32;
                tile = CeresStationAtlas.TwoRowStrip(CeresStationAtlas.Shoulder, column);
            }
            else
            {
                int column = 2 * (40 - index);
                x = -10 + 8 * column; y = -48; tile = CeresStationAtlas.Top + column;
            }
            return new(SnesSpritemapXWord.Create(x, index is 1 or 2 or >= 13), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }

    /// <summary>Enumerates all spritemap parts in the order used by the native publication.</summary>
    /// <returns>An enumerator over the compiled parts from index zero through the final part.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Names the tile indices and layout helpers used to assemble the Ceres station spritemap from packed atlas strips.</summary>
internal static class CeresStationAtlas
{
    /// <summary>TileE6, top strip at Y=-48 in8C:9150.</summary>
    internal const int Top = 0xe6;
    /// <summary>TileEC, left end of the shoulder strip at Y=-32.</summary>
    internal const int Shoulder = 0xec;
    /// <summary>Tile107, left end of the upper-middle strip at Y=-16.</summary>
    internal const int UpperMiddle = 0x107;
    /// <summary>Tile124, left end of the middle strip at Y=0.</summary>
    internal const int Middle = 0x124;
    /// <summary>Tile140, left end of the lower strip at Y=16.</summary>
    internal const int Lower = 0x140;
    /// <summary>TileF0, small extension beyond the right end of the middle strip.</summary>
    internal const int RightTip = 0xf0;
    /// <summary>TileF1, two adjacent lower-right edge cells followed by the
    /// separately packed downward continuation of the neighboring pair.</summary>
    internal const int LowerRightEdge = 0xf1;
    /// <summary>Tile15B, two adjacent small cells on the right of the lower center patch.</summary>
    internal const int LowerRightPair = 0x15b;
    /// <summary>Tile14B, three-cell left edge, its lower-right continuation in the
    /// next atlas row, and an adjoining large center patch at column+3.</summary>
    internal const int LowerLeftEdge = 0x14b;
    /// <summary>At a sixteen-column boundary, skip the row of lower tile halves.</summary>
    internal static int TwoRowStrip(int start, int column)
    {
        int atlasColumn = (start & 15) + column;
        return (start & ~15) + 32 * (atlasColumn / 16) + atlasColumn % 16;
    }
}
