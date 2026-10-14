using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>All fifty parts of8C:9558 calculated from packed horizontal bands,
/// top/bottom rims and adjoining edge cells. Preserve the original upper-band
/// atlas seam, including its documented one-cell-left tile mismatch.</summary>
internal sealed class ZebesPlanetBandParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces the planet backdrop only when supplied art matches the calculated native fifty-part layout.</summary>
    internal static SpriteComposition CalculateIfMatching(CeresDestructionBackdrop? backdrop, SpriteComposition supplied) =>
        backdrop == CeresDestructionBackdrop.Planet
            ? supplied.CalculateIfMatching(new ZebesPlanetBandParts()) : supplied;
    /// <summary>Includes small rim cells and large horizontal-band pieces in native draw order.</summary>
    public int Count => 50;
    /// <summary>Calculates atlas tile and screen offset for a planet rim, cap, or horizontal-band piece.</summary>
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
    /// <summary>Visits the calculated fifty-part planet silhouette in cartridge OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same planet drawing order through the non-generic interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
