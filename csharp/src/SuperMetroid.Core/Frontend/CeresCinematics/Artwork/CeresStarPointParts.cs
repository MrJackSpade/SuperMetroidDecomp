using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Calculate the common small-sprite attributes of Ceres star sheets.
/// Point positions and selected star drawings remain a separate content mapping.</summary>
internal sealed class CeresStarPointParts(StarfieldPoint[] points, TileFlip flips)
    : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Reconstructs common attributes only for the 25-part Ceres star composition; otherwise retains supplied artwork.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        if (shown != CeresFlightSpriteDefinitions.Stars || supplied.PartCount != 25) return supplied;
        var parts = Enumerable.Range(0, supplied.PartCount).Select(supplied.Part).ToArray();
        var selected = CalculateIfMatching(parts, TileFlip.Horizontal);
        return ReferenceEquals(parts, selected) ? supplied : supplied.CalculateIfMatching(selected);
    }
    /// <summary>Uses calculated common attributes only when all supplied parts already agree with them.</summary>
    internal static IReadOnlyList<CompiledSpritePart> CalculateIfMatching(IReadOnlyList<CompiledSpritePart> supplied, TileFlip flips)
    {
        var points = new StarfieldPoint[supplied.Count];
        for (int index = 0; index < points.Length; index++)
        {
            var part = supplied[index];
            points[index] = new(Offset16<SpritePixels>.Of(part.Part.Offset.X, part.Part.Offset.Y), part.Part.Character);
        }
        var calculated = new CeresStarPointParts(points, flips);
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
            return new(SpritePart.Create(point.Offset.X, point.Offset.Y, false, point.Tile, 0, 0, flips), true);
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
