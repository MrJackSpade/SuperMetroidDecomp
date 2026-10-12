using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native8C:8C8D..8CCE mirrors one eight-pixel tile into four quadrants.
/// Preserve bottom-right,bottom-left,top-right,top-left order and actor palette.</summary>
internal sealed class IntroRinkaParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Calculates if matching.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroRinkaSpriteDefinitions.FramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroRinkaParts(frame));
        return supplied;
    }
    /// <summary>Gets count.</summary>
    public int Count => 4;
    /// <summary>Gets an indexed value.</summary>
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
    /// <summary>Returns an enumerator over this sequence.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns an enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Represents intro rinka atlas.</summary>
internal static class IntroRinkaAtlas
{
    /// <summary>Tile$196, first of three adjacent top-left Rinka quadrant drawings.</summary>
    internal const int FirstQuadrant = 0x196;
}
