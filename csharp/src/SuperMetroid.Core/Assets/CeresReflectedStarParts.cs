using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The23 stars in8C:8FE7 reflect parts2..24 of8C:9478 horizontally.
/// The first two source stars would overlap the vortex drawing and are omitted.
/// Preserve fixed source ordering; this is not dynamic clipping of edited stars.</summary>
internal sealed class CeresReflectedStarParts(SpriteComposition source) : IReadOnlyList<CompiledSpritePart>
{
    internal static IReadOnlyList<CompiledSpritePart> CalculateIfMatching(SpriteComposition? source, IReadOnlyList<CompiledSpritePart> supplied)
    {
        if (source is null || source.PartCount != 25 || supplied.Count != 23) return supplied;
        var calculated = new CeresReflectedStarParts(source);
        return supplied.SequenceEqual(calculated) ? calculated : supplied;
    }
    public int Count => 23;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var part = source.Part(index + 2);
            int size = part.X.IsLarge ? 16 : 8;
            int x = ((-part.X.SignedOffset - size + 256) & 511) - 256;
            return new(SnesSpritemapXWord.Create(x, part.X.IsLarge), part.Y,
                SnesObjAttributeWord.Create(part.Attributes.TileNumber, part.Attributes.PaletteIndex,
                    part.Attributes.Priority, part.Attributes.FlipFlags ^ SnesTileFlipFlags.Horizontal), part.InheritPalette);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}