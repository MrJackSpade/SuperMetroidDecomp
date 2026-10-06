using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A396..A471: eight planet grids and two mirrored lava grids.
/// Planet tiles advance four columns per pose and four tile rows per four-pose group.
/// Parts draw bottom-right to top-left; lava pose two instead traverses columns.
/// Palette bits inherit the actor, as in the native generic spritemap loader.</summary>
internal sealed class EndingExplosionGridParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly int pose;
    private EndingExplosionGridParts(int pose) => this.pose = pose;

    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        int offset = pointer - EndingExplosionSpriteDefinitions.Pointer(EndingExplosionSpriteDefinitions.Pose.DamageFirst);
        const int recordBytes = 2 + 4 * 5;
        if ((uint)offset >= 10 * recordBytes || offset % recordBytes != 0) return supplied;
        return supplied.CalculateIfMatching(new EndingExplosionGridParts(offset / recordBytes));
    }

    public int Count => 4;
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
            SnesTileFlipFlags flips = lava ?
                (column != 0 ? SnesTileFlipFlags.Horizontal : 0) |
                (row != 0 ? SnesTileFlipFlags.Vertical : 0) : 0;
            return new(SnesSpritemapXWord.Create((column - 1) * 16, true),
                unchecked((byte)((row - 1) * 16)), SnesObjAttributeWord.Create(tile, 0, 0, flips), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
