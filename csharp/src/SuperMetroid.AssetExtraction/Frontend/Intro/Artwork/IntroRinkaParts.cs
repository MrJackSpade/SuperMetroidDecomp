using System.Collections;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Native8C:8C8D..8CCE mirrors one eight-pixel tile into four quadrants.
/// Preserve bottom-right,bottom-left,top-right,top-left order and actor palette.</summary>
internal sealed class IntroRinkaParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated native quadrants only for a matching Rinka pose and exact supplied visual fields; edits remain supplied.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroRinkaSpritePoses.FramePose(frame) == shown)
                return supplied.CalculateIfMatching(new IntroRinkaParts(frame));
        return supplied;
    }
    /// <summary>Four mirrored quadrants compose each intro Rinka frame.</summary>
    public int Count => 4;
    /// <summary>Selects a checked quadrant in native draw order, applying flips while inheriting the actor palette.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int column = index % 2, row = index / 2;
            TileFlip flips = (column == 0 ? TileFlip.Horizontal : 0)
                | (row == 0 ? TileFlip.Vertical : 0);
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(-8 * column, -8 * row), false,
                IntroRinkaCellDefinitions.FirstQuadrant + frame, 0, 3, flips), true);
        }
    }
    /// <summary>Yields bottom-right through top-left quadrants in native composition order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same four-part ordering through the nongeneric collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
