using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Atlas packing and sprite geometry for subtitle arrows, delivered/examined baby and caret.</summary>
internal sealed class IntroScientistParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Recalculates one of ten arrow, baby, or caret frames when its pose matches.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 10; frame++)
            if (IntroScientistSpriteDefinitions.FramePose(frame) == shown)
                return supplied.CalculateIfMatching(new IntroScientistParts(frame));
        return supplied;
    }
    /// <summary>Arrow poses use mirrored halves, delivered baby poses six parts, and examination or caret poses one.</summary>
    public int Count => frame < 3 ? 2 : frame < 6 ? 6 : 1;
    /// <summary>Assembles a frame part from the arrow, delivered-baby, examined-baby, or caret atlas region.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large = false;
            TileFlip flips = 0;
            if (frame < 3)
            {
                x = -1 - 7 * index;
                y = 0;
                flips = index == 0 ? TileFlip.Horizontal : 0;
                tile = frame == 0 ? IntroScientistCellDefinitions.InitialArrow
                    : IntroScientistCellDefinitions.ArrowColumn + 16 * (frame - 1);
            }
            else if (frame < 6)
            {
                int pose = frame - 3;
                if (index < 3)
                {
                    x = 4 - 8 * index;
                    y = -12;
                    tile = IntroScientistCellDefinitions.DeliveredTop + 3 * (pose % 2) + 16 * (pose / 2) + 2 - index;
                }
                else
                {
                    // Each lower 3x2 patch uses one 2x2 sprite and a two-sprite side column.
                    // The middle pose puts that column on the left, preserving native OAM order.
                    bool leftColumn = (pose & 1) != 0;
                    large = index == 5;
                    x = large ? -12 + (leftColumn ? 8 : 0) : leftColumn ? -12 : 4;
                    y = index == 3 ? 4 : -4;
                    int column = large ? (leftColumn ? 1 : 0) : leftColumn ? 0 : 2;
                    tile = IntroScientistCellDefinitions.DeliveredLower + 3 * pose + column + (index == 3 ? 16 : 0);
                }
            }
            else if (frame < 9)
            {
                x = y = -8;
                large = true;
                tile = IntroScientistCellDefinitions.ExaminedStart + 2 * (frame - 6);
            }
            else
            {
                x = 0;
                y = -1;
                tile = IntroScientistCellDefinitions.Caret;
            }
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), large,
                tile, 0, 3, flips), true);
        }
    }
    /// <summary>Enumerates scene parts in the native OAM drawing order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the same part order for non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
