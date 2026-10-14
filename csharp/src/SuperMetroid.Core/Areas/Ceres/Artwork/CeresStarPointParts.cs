using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Calculate the common small-sprite attributes of Ceres star sheets.
/// Point positions and selected star drawings remain a separate content mapping.</summary>
internal sealed class CeresStarPointParts((int X, byte Y, int Tile)[] points, bool reflected)
    : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Reconstructs common attributes only for the 25-part Ceres star composition; otherwise retains supplied artwork.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != CeresFlightSpriteDefinitions.Stars || supplied.PartCount != 25) return supplied;
        var parts = Enumerable.Range(0, supplied.PartCount).Select(supplied.Part).ToArray();
        var selected = CalculateIfMatching(parts, true);
        return ReferenceEquals(parts, selected) ? supplied : supplied.CalculateIfMatching(selected);
    }
    /// <summary>Uses calculated common attributes only when all supplied parts already agree with them.</summary>
    internal static IReadOnlyList<CompiledSpritePart> CalculateIfMatching(IReadOnlyList<CompiledSpritePart> supplied, bool reflected)
    {
        var points = new (int X, byte Y, int Tile)[supplied.Count];
        for (int index = 0; index < points.Length; index++)
        {
            var part = supplied[index];
            points[index] = (part.X.SignedOffset, part.Y, part.Attributes.TileNumber);
        }
        var calculated = new CeresStarPointParts(points, reflected);
        return supplied.SequenceEqual(calculated) ? calculated : supplied;
    }
    /// <summary>Number of points in the source star composition.</summary>
    public int Count => points.Length;
    /// <summary>Builds a star sprite part with the requested horizontal reflection and source tile number.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var point = points[index];
            return new(SnesSpritemapXWord.Create(point.X, false), point.Y,
                SnesObjAttributeWord.Create(point.Tile, 0, 0, reflected ? SnesTileFlipFlags.Horizontal : 0), true);
        }
    }
    /// <summary>Enumerates the calculated star parts in source order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same ordered star parts through non-generic enumeration.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
