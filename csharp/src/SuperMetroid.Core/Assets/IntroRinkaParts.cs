using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native8C:8C8D..8CCE mirrors one eight-pixel tile into four quadrants.
/// Preserve bottom-right,bottom-left,top-right,top-left order and actor palette.</summary>
internal sealed class IntroRinkaParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroRinkaSpriteDefinitions.FramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroRinkaParts(frame));
        return supplied;
    }
    public int Count => 4;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int column = index % 2, row = index / 2;
            SnesTileFlipFlags flips = (column == 0 ? SnesTileFlipFlags.Horizontal : 0)
                | (row == 0 ? SnesTileFlipFlags.Vertical : 0);
            return new(SnesSpritemapXWord.Create(-8 * column, false), unchecked((byte)(-8 * row)),
                SnesObjAttributeWord.Create(IntroRinkaAtlas.FirstQuadrant + frame, 0, 3, flips), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class IntroRinkaAtlas
{
    /// <summary>Tile$196, first of three adjacent top-left Rinka quadrant drawings.</summary>
    internal const int FirstQuadrant = 0x196;
}
