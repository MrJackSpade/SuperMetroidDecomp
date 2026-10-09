using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native8C:8C8D..8CCE mirrors one eight-pixel tile into four quadrants.
/// Preserve bottom-right,bottom-left,top-right,top-left order and actor palette.</summary>
internal sealed class IntroRinkaParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces recognized Rinka frame pointers with the four-part mirrored-tile composition.</summary>
    /// <param name="pointer">Source spritemap pointer tested against the three compiled Rinka frames.</param>
    /// <param name="supplied">Composition parsed from the original spritemap.</param>
    /// <returns>The compiled quadrant composition for a matching pointer, or the supplied composition otherwise.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroRinkaSpriteDefinitions.FramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroRinkaParts(frame));
        return supplied;
    }
    /// <summary>Gets the number of quadrant parts used to form one Rinka tile.</summary>
    public int Count => 4;

    /// <summary>Gets a quadrant with its position, reflection, shared tile, and actor palette.</summary>
    /// <param name="index">Zero-based quadrant index in bottom-right, bottom-left, top-right, top-left order.</param>
    /// <returns>The compiled sprite part for the requested quadrant.</returns>
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
    /// <summary>Enumerates the four quadrants in the composition's native order.</summary>
    /// <returns>An enumerator over the Rinka sprite parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Identifies the adjacent atlas tiles that supply the Rinka quadrant drawings.</summary>
internal static class IntroRinkaAtlas
{
    /// <summary>Tile$196, first of three adjacent top-left Rinka quadrant drawings.</summary>
    internal const int FirstQuadrant = 0x196;
}
