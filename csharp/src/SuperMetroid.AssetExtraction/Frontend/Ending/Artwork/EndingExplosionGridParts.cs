using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>$8C:A396..A471: eight planet grids and two mirrored lava grids.
/// Planet tiles advance four columns per pose and four tile rows per four-pose group.
/// Parts draw bottom-right to top-left; lava pose two instead traverses columns.
/// Palette bits inherit the actor, as in the native generic spritemap loader.</summary>
internal sealed class EndingExplosionGridParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>The native planet or lava pose whose grid will be reconstructed.</summary>
    private readonly int pose;
    /// <summary>Captures a bounded native pose index for the four-part grid.</summary>
    private EndingExplosionGridParts(int pose) => this.pose = pose;

    /// <summary>Reconstructs grid parts only for the ten contiguous planet and lava records.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        for (int record = 0; record < 10; record++)
            if (shown == EndingExplosionSpritePoses.PlanetPose(record))
                return supplied.CalculateIfMatching(new EndingExplosionGridParts(record));
        return supplied;
    }

    /// <summary>Each native pose draws a two-by-two tile grid.</summary>
    public int Count => 4;
    /// <summary>Calculates tile order, position, and lava reflections for a native grid part.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int column = 1 - (pose == 9 ? index / 2 : index % 2);
            int row = 1 - (pose == 9 ? index % 2 : index / 2);
            bool lava = pose >= 8;
            int tile = lava ? 0x99 + (pose - 8) * 2 : // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
                (pose / 4) * 0x40 + (pose % 4) * 4 + row * 0x20 + column * 2; // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
            TileFlip flips = lava ?
                (column != 0 ? TileFlip.Horizontal : 0) |
                (row != 0 ? TileFlip.Vertical : 0) : 0;
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of((column - 1) * 16, (row - 1) * 16), true, tile, 0, 0, flips), true);
        }
    }
    /// <summary>Yields the four parts in native drawing order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes native drawing order through non-generic enumeration.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
