using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculate the common small-sprite attributes of Ceres star sheets.
/// Point positions and selected star drawings remain a separate content mapping.</summary>
internal sealed class CeresStarPointParts((int X, byte Y, int Tile)[] points, bool reflected)
    : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != CeresFlightSpriteDefinitions.Stars || supplied.PartCount != 25) return supplied;
        var parts = Enumerable.Range(0, supplied.PartCount).Select(supplied.Part).ToArray();
        var selected = CalculateIfMatching(parts, true);
        return ReferenceEquals(parts, selected) ? supplied : supplied.CalculateIfMatching(selected);
    }
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
    public int Count => points.Length;
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
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}