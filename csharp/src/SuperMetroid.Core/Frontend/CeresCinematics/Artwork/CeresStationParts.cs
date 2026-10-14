using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>All forty-one parts of8C:9150 from packed body strips, edge patches
/// and their adjoining cells. Preserve the native publication order.</summary>
internal sealed class CeresStationParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated station pieces only for the matching under-attack frame pointer.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied) =>
        pointer == CeresFlightSpriteDefinitions.StationUnderAttack
            ? supplied.CalculateIfMatching(new CeresStationParts()) : supplied;
    /// <summary>Native under-attack station composition spans forty-one OAM parts.</summary>
    public int Count => 41;
    /// <summary>Calculates strip, shoulder, edge patch, or cap tile placement in native drawing order.</summary>
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
                tile = CeresStationCellDefinitions.Top - 1 + (index == 0 ? 15 : 0);
            }
            else if (index == 2)
            {
                x = -50 + 8 * 9; y = 16; tile = CeresStationCellDefinitions.Lower + 9;
            }
            else if (index == 3)
            {
                // Small continuation after the six-part middle strip.
                x = -50 + 6 * 16; y = 0; tile = CeresStationCellDefinitions.RightTip;
            }
            else if (index < 6)
            {
                int column = 5 - index;
                x = -50 + 8 * (8 + column); y = 32;
                tile = CeresStationCellDefinitions.LowerRightEdge + column;
            }
            else if (index == 6)
            {
                // Downward continuation of the left cell of the lower-right pair.
                x = -50 + 6 * 8; y = 32 + 8;
                tile = CeresStationCellDefinitions.LowerRightEdge + 2;
            }
            else if (index < 9)
            {
                int column = 8 - index;
                x = -50 + 8 * (6 + column); y = 32;
                tile = CeresStationCellDefinitions.LowerRightPair + column;
            }
            else if (index < 13)
            {
                // Three-cell left edge plus a downward continuation of its right cell.
                int column = index == 9 ? 2 : 12 - index;
                int row = index == 9 ? 1 : 0;
                x = -50 + 8 * (1 + column); y = 32 + 8 * row;
                tile = CeresStationCellDefinitions.LowerLeftEdge + column + 16 * row;
            }
            else if (index == 13)
            {
                // The large center patch immediately follows the three small edge cells.
                x = -50 + 4 * 8; y = 32; tile = CeresStationCellDefinitions.LowerLeftEdge + 3;
            }
            else if (index < 19)
            {
                int column = 2 * (18 - index);
                x = -50 + 8 * column; y = 16; tile = CeresStationCellDefinitions.Lower + column;
            }
            else if (index < 25)
            {
                int column = 2 * (24 - index);
                x = -50 + 8 * column; y = 0; tile = CeresStationCellDefinitions.Middle + column;
            }
            else if (index < 32)
            {
                int piece = 31 - index;
                int column = 2 * piece - (piece >= 4 ? 1 : 0);
                x = -50 + 8 * column; y = -16;
                tile = CeresStationCellDefinitions.TwoRowStrip(CeresStationCellDefinitions.UpperMiddle, column);
            }
            else if (index < 38)
            {
                int column = Math.Min(2 * (37 - index), 9);
                x = -42 + 8 * column; y = -32;
                tile = CeresStationCellDefinitions.TwoRowStrip(CeresStationCellDefinitions.Shoulder, column);
            }
            else
            {
                int column = 2 * (40 - index);
                x = -10 + 8 * column; y = -48; tile = CeresStationCellDefinitions.Top + column;
            }
            return new(SnesSpritemapXWord.Create(x, index is 1 or 2 or >= 13), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }
    /// <summary>Enumerates the station pieces in their authored OAM overlap order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same station overlap order to non-generic callers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
