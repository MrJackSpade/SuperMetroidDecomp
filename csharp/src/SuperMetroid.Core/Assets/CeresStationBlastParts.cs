using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Six station-blast layouts from 8C:98EE..99D5. Generate reflected
/// quadrants and peripheral tips in native OAM order; retain no full-part table.</summary>
/// <param name="frame">Native station-blast frame index selecting the generated part layout.</param>
internal sealed class CeresStationBlastParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Calculates the supplied composition with generated station-blast parts when its pointer identifies one of the six native frames.</summary>
    /// <param name="pointer">Compiled spritemap pointer to check against the station-blast frame table.</param>
    /// <param name="supplied">Composition whose existing calculation handles nonmatching pointers.</param>
    /// <returns>The matching composition after station-blast parts are supplied, or the unchanged result for other pointers.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 6; frame++)
            if (pointer == CeresDestructionSpriteDefinitions.StationFrame(frame))
                return supplied.CalculateIfMatching(new CeresStationBlastParts(frame));
        return supplied;
    }

    /// <summary>Gets the number of sprite parts generated for the selected native frame.</summary>
    public int Count => CeresDestructionSpriteDefinitions.StationPartCount(frame);

    /// <summary>Gets a generated sprite part at its native OAM-order index.</summary>
    /// <param name="index">Zero-based part index within this frame.</param>
    /// <returns>The part's position, tile attributes, and size.</returns>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile, size;
            if (frame < 3)
            {
                size = frame == 0 ? 8 : 16;
                int column = frame == 1 ? index / 2 : index % 2;
                int row = frame == 1 ? index % 2 : index / 2;
                x = -size * column; y = -size * row;
                tile = frame == 0 ? IntroMotherBrainExplosionAtlas.BigInitialQuadrant
                    : IntroMotherBrainExplosionAtlas.BigQuadrantStart + 2 * (frame - 1);
            }
            else if (frame == 3)
            {
                if (index < 8)
                {
                    size = 8;
                    int side = index / 2, half = index % 2;
                    bool verticalPair = (side & 1) == 0;
                    x = verticalPair ? (side == 0 ? 16 : -24) : -8 * half;
                    y = verticalPair ? -8 * half : (side == 1 ? 16 : -24);
                    tile = CeresStationBlastAtlas.ExpandedTips + (verticalPair ? 16 : 0);
                }
                else
                {
                    size = 16;
                    int quadrant = index - 8;
                    x = -16 * (quadrant / 2); y = -16 * (quadrant % 2);
                    tile = CeresStationBlastAtlas.ExpandedCore;
                }
            }
            else if (frame == 4)
            {
                size = 16;
                int quadrant = index / 2, lobe = index % 2;
                int column = quadrant % 2, row = quadrant / 2;
                x = lobe == 0 ? 8 : 0; y = lobe == 0 ? 0 : 8;
                if (column != 0) x = -size - x;
                if (row != 0) y = -size - y;
                tile = CeresStationBlastAtlas.SplitLobes + 2 * (1 - lobe);
            }
            else
            {
                if (index < 8)
                {
                    size = 8;
                    bool horizontalPair = index < 4;
                    int piece = index % 4;
                    int near = -8 * (piece % 2), far = piece < 2 ? 16 : -24;
                    x = horizontalPair ? near : far; y = horizontalPair ? far : near;
                    tile = horizontalPair ? CeresStationBlastAtlas.DetachedVerticalTips : CeresStationBlastAtlas.DetachedHorizontalTips;
                }
                else
                {
                    size = 16;
                    int quadrant = index - 8;
                    x = quadrant % 2 == 0 ? 8 : -24;
                    y = quadrant / 2 == 0 ? 8 : -24;
                    tile = CeresStationBlastAtlas.DetachedCore;
                }
            }
            var flips = (x >= 0 ? SnesTileFlipFlags.Horizontal : 0)
                | (y >= 0 ? SnesTileFlipFlags.Vertical : 0);
            return new(SnesSpritemapXWord.Create(x, size == 16), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, flips), true);
        }
    }

    /// <summary>Enumerates the generated parts in native OAM order.</summary>
    /// <returns>An enumerator that creates each part on demand.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Atlas tile indices used to compose expanded and detached Ceres station-blast frames.</summary>
internal static class CeresStationBlastAtlas
{
    /// <summary>Tile B0, large core quadrant of native frame 8C:9930.</summary>
    internal const int ExpandedCore = 0xb0;
    /// <summary>Tile B2, vertical tip; the horizontal tip occupies the next atlas row.</summary>
    internal const int ExpandedTips = 0xb2;
    /// <summary>Tile B3, first of two adjacent large lobe drawings in 8C:996E.</summary>
    internal const int SplitLobes = 0xb3;
    /// <summary>Tile B7, horizontal peripheral tip of native frame 8C:9998.</summary>
    internal const int DetachedHorizontalTips = 0xb7;
    /// <summary>Tile B8, large detached diagonal quadrant of native frame 8C:9998.</summary>
    internal const int DetachedCore = 0xb8;
    /// <summary>Tile BB, vertical peripheral tip of native frame 8C:9998.</summary>
    internal const int DetachedVerticalTips = 0xbb;
}
