using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The23 stars in8C:8FE7 reflect parts2..24 of8C:9478 horizontally.
/// The first two source stars would overlap the vortex drawing and are omitted.
/// Preserve fixed source ordering; this is not dynamic clipping of edited stars.</summary>
/// <param name="source">Source star composition whose trailing parts are reflected.</param>
internal sealed class CeresReflectedStarParts(SpriteComposition source) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Returns the supplied parts unless they match the fixed 23-part horizontal reflection of a 25-part source composition.</summary>
    /// <param name="source">Candidate source composition; only compositions with the expected 25-part shape can produce reflected stars.</param>
    /// <param name="supplied">Existing parts compared in order with the fixed reflected-star calculation.</param>
    /// <returns>The calculated reflection when it matches <paramref name="supplied"/>; otherwise, the original supplied list.</returns>
    internal static IReadOnlyList<CompiledSpritePart> CalculateIfMatching(SpriteComposition? source, IReadOnlyList<CompiledSpritePart> supplied)
    {
        if (source is null || source.PartCount != 25 || supplied.Count != 23) return supplied;
        var calculated = new CeresReflectedStarParts(source);
        return supplied.SequenceEqual(calculated) ? calculated : supplied;
    }
    /// <summary>Gets the fixed number of reflected star parts, omitting the first two source stars.</summary>
    public int Count => 23;

    /// <summary>Gets a star part reflected horizontally from source part <c>index + 2</c>.</summary>
    /// <param name="index">Zero-based position in the 23-part reflected sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the reflected sequence.</exception>
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
    /// <summary>Enumerates the 23 reflected star parts in their fixed source order.</summary>
    /// <returns>An enumerator over the reflected parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
