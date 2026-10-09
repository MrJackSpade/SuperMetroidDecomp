using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculate the common small-sprite attributes of Ceres star sheets.
/// Point positions and selected star drawings remain a separate content mapping.</summary>
internal sealed class CeresStarPointParts((int X, byte Y, int Tile)[] points, bool reflected)
    : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces the matching Ceres star spritemap with its compact point-based representation.</summary>
    /// <param name="pointer">Spritemap pointer used to identify the Ceres star definition.</param>
    /// <param name="supplied">Composition produced from the original spritemap.</param>
    /// <returns>The supplied composition when it is not the expected star definition; otherwise, a composition using the point-based parts when they match.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != CeresFlightSpriteDefinitions.Stars || supplied.PartCount != 25) return supplied;
        var parts = Enumerable.Range(0, supplied.PartCount).Select(supplied.Part).ToArray();
        var selected = CalculateIfMatching(parts, true);
        return ReferenceEquals(parts, selected) ? supplied : supplied.CalculateIfMatching(selected);
    }
    /// <summary>Builds point-based parts from the supplied sprite attributes when their calculated values match.</summary>
    /// <param name="supplied">Parts whose positions and tile numbers provide the source points.</param>
    /// <param name="reflected">Whether the generated parts should use horizontal tile reflection.</param>
    /// <returns>The calculated point-based parts if they match the supplied sequence; otherwise, the original sequence.</returns>
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
    /// <summary>Gets the number of stored star points, each of which produces one sprite part.</summary>
    public int Count => points.Length;

    /// <summary>Gets the sprite part generated from the point at the specified zero-based position.</summary>
    /// <param name="index">Zero-based point position to convert into a sprite part.</param>
    /// <returns>A sprite part with the point's position and tile, applying the configured reflection.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the point sequence.</exception>
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

    /// <summary>Enumerates the sprite parts generated from the stored points in their original order.</summary>
    /// <returns>An enumerator over the generated sprite parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
