using System.Collections;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Six station-blast layouts from 8C:98EE..99D5. Generate reflected
/// quadrants and peripheral tips in native OAM order; retain no full-part table.</summary>
internal sealed class CeresStationBlastParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated reflected parts only for one of the six native station-blast compositions.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 6; frame++)
            if (pointer == CeresDestructionSpriteDefinitions.StationFrame(frame))
                return supplied.CalculateIfMatching(new CeresStationBlastParts(frame));
        return supplied;
    }
    /// <summary>Uses each animation frame's native OAM part count.</summary>
    public int Count => CeresDestructionSpriteDefinitions.StationPartCount(frame);
    /// <summary>Calculates one core, lobe, or detached tip from frame geometry and atlas tiles.</summary>
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
    /// <summary>Emits all calculated blast parts in native OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Preserves that OAM order for non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
