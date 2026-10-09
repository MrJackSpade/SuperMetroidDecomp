using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>All fifty parts of8C:9558 calculated from packed horizontal bands,
/// top/bottom rims and adjoining edge cells. Preserve the original upper-band
/// atlas seam, including its documented one-cell-left tile mismatch.</summary>
internal sealed class ZebesPlanetBandParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces the supplied sprite composition with the calculated planet band only for its native frame pointer.</summary>
    /// <param name="pointer">Native bank-$8C spritemap pointer being composed.</param>
    /// <param name="supplied">Existing composition retained when the pointer is not the planet frame.</param>
    /// <returns>The composition including calculated band parts for the matching frame, or <paramref name="supplied"/> unchanged.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied) =>
        pointer == CeresDestructionSpriteDefinitions.Planet
            ? supplied.CalculateIfMatching(new ZebesPlanetBandParts()) : supplied;

    /// <summary>Number of compiled small- and large-sprite parts covering the planet's band and edge patches.</summary>
    public int Count => 50;

    /// <summary>Builds one compiled OBJ part from the indexed band, rim, or adjoining edge cell.</summary>
    /// <param name="index">Zero-based part position in the 50-entry composition.</param>
    /// <returns>Position, atlas tile, and OBJ attributes for the selected part.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the composition.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large = index is >= 15 and < 40 or >= 41;
            if (index == 0)
            {
                // One small cell immediately left of the lower large-sprite band.
                x = -48 - 8; y = 16; tile = ZebesPlanetBandAtlas.South - 1;
            }
            else if (index < 4)
            {
                // Three cells of the northwest two-by-two patch; top-left is empty.
                int cell = 4 - index;
                x = -48 + 8 * (cell % 2); y = -64 + 8 * (cell / 2);
                tile = ZebesPlanetBandAtlas.Northwest + cell % 2 + 16 * (cell / 2);
            }
            else if (index == 4)
            {
                // Lower-left cell adjoining the northern large-sprite band.
                x = -56 - 8; y = -48 + 8; tile = ZebesPlanetBandAtlas.North - 1 + 16;
            }
            else if (index < 12)
            {
                int column = 11 - index;
                x = -40 + 8 * column; y = 32; tile = ZebesPlanetBandAtlas.BottomRim + column;
            }
            else if (index < 15)
            {
                int cell = 14 - index;
                x = 16 + 8 * (cell % 2); y = 16 + 8 * (cell / 2);
                tile = ZebesPlanetBandAtlas.Southeast + cell % 2 + 16 * (cell / 2);
            }
            else if (index < 19)
            {
                int column = 2 * (18 - index);
                x = -48 + 8 * column; y = 16;
                tile = ZebesPlanetBandAtlas.TwoRowStrip(ZebesPlanetBandAtlas.South, column);
            }
            else if (index < 33)
            {
                int band = (index - 19) / 7;
                int column = Math.Min(2 * (6 - (index - 19) % 7), 11);
                x = -64 + 8 * column; y = -16 * band;
                tile = (band == 0 ? ZebesPlanetBandAtlas.Lower : ZebesPlanetBandAtlas.Upper) + column;
            }
            else if (index < 40)
            {
                int piece = 39 - index;
                x = -64 + 8 * Math.Min(2 * piece, 11); y = -32;
                tile = piece == 6 ? ZebesPlanetBandAtlas.UpperRightOverlap
                    : ZebesPlanetBandAtlas.UpperShoulder + 2 * piece - (piece == 5 ? 1 : 0);
            }
            else if (index == 40)
            {
                // Bottom cell immediately to the right of the three-part top cap.
                x = 16; y = -64 + 8; tile = ZebesPlanetBandAtlas.TopCap + 6 + 16;
            }
            else if (index < 47)
            {
                int column = Math.Min(2 * (46 - index), 9);
                x = -56 + 8 * column; y = -48;
                tile = ZebesPlanetBandAtlas.TwoRowStrip(ZebesPlanetBandAtlas.North, column);
            }
            else
            {
                int column = 2 * (49 - index);
                x = -32 + 8 * column; y = -64; tile = ZebesPlanetBandAtlas.TopCap + column;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }
    /// <summary>Enumerates all 50 compiled parts in the order expected by the Ceres destruction composition.</summary>
    /// <returns>An enumerator over the calculated band and edge parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
/// <summary>Tile-column anchors and stride calculation for the packed Ceres planet-band atlas.</summary>
internal static class ZebesPlanetBandAtlas
{
    /// <summary>Tile00, northwest two-by-two small-cell patch, with empty top-left.</summary>
    internal const int Northwest = 0x00;
    /// <summary>Tile02, left end of the three-large-part cap at Y=-64.</summary>
    internal const int TopCap = 0x02;
    /// <summary>Tile0A, left end of the northern two-row strip at Y=-48.</summary>
    internal const int North = 0x0a;
    /// <summary>Tile25, left end of the upper shoulder at Y=-32. Native part34
    /// uses2E instead of the stride's2F, preserving the pinned disassembly's tiling error.</summary>
    internal const int UpperShoulder = 0x25;
    /// <summary>Tile41, separately packed overlapping right edge of the upper shoulder.</summary>
    internal const int UpperRightOverlap = 0x41;
    /// <summary>Tile60, left edge of the planet band at relative Y=0 in8C:9558.</summary>
    internal const int Lower = 0x60;
    /// <summary>Tile43, left edge of the planet band at relative Y=-16 in8C:9558.</summary>
    internal const int Upper = 0x43;
    /// <summary>Tile6E, left end of the southern two-row strip at Y=16.</summary>
    internal const int South = 0x6e;
    /// <summary>Tile86, southeast three-cell patch, with empty bottom-right.</summary>
    internal const int Southeast = 0x86;
    /// <summary>Tile88, left end of the seven-small-part bottom rim at Y=32.</summary>
    internal const int BottomRim = 0x88;

    /// <summary>Advance a horizontal strip through atlas columns; crossing sixteen
    /// columns skips the atlas row occupied by the bottom halves of large parts.</summary>
    internal static int TwoRowStrip(int start, int column)
    {
        int atlasColumn = (start & 15) + column;
        return (start & ~15) + 32 * (atlasColumn / 16) + atlasColumn % 16;
    }
}
