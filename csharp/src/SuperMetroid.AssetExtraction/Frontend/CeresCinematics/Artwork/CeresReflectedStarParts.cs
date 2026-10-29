using System.Collections;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>The 23 stars in $8C:8FE7 reflect parts 2..24 of $8C:9478 horizontally.
/// The first two source stars would overlap the vortex drawing and are omitted.
/// Preserve fixed source ordering; this is not dynamic clipping of edited stars.</summary>
internal sealed class CeresReflectedStarParts(SpriteComposition source) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated reflection only when both composition sizes and every supplied part match the native construction.</summary>
    internal static IReadOnlyList<CompiledSpritePart> CalculateIfMatching(SpriteComposition? source, IReadOnlyList<CompiledSpritePart> supplied)
    {
        if (source is null || source.PartCount != 25 || supplied.Count != 23) return supplied;
        var calculated = new CeresReflectedStarParts(source);
        return supplied.SequenceEqual(calculated) ? calculated : supplied;
    }
    /// <summary>Native reflected-star count after omitting the first two source stars.</summary>
    public int Count => 23;
    /// <summary>Mirrors a source star's signed X position and horizontal flip while preserving its size, tile, palette, and Y position.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var part = source.Part(index + 2);
            int size = part.Part.Large ? 16 : 8;
            int x = SpritePart.WrapX(-part.Part.Offset.X - size);
            return new(part.Part.At(Offset16<ScreenPixels>.Of(x, part.Part.Offset.Y)) with { Flip = part.Part.Flip ^ TileFlip.Horizontal },
                part.InheritPalette);
        }
    }
    /// <summary>Yields reflected source parts 2..24 in their original order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the same reflected-star order for non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
