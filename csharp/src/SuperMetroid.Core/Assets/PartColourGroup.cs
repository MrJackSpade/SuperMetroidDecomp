using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colour group of the part a pixel of an installed image belongs to, and whether that part kept its own colour group
/// when the import composed it or followed the palette its spritemap was drawn with (#1464). The import pipeline makes a
/// kept part of a part naming its own colours in a family whose parts keep them and of every background pixel. The
/// installed sheet format records it for each pixel; no draw recolours by it, as every draw shows its picture under the
/// shading its owner names (#1512).
/// </summary>
/// <remarks>
/// The installed image stores each pixel's part group as its <see cref="Code"/>: the group's place, raised by
/// <see cref="ImageColourGroup.Count"/> where the part keeps its colours.
/// </remarks>
public readonly record struct PartColourGroup
{
    /// <summary>The part groups a pixel names: each colour group, followed and kept.</summary>
    public const int Codes = 2 * ImageColourGroup.Count;

    /// <summary>Names the part group the installed image stores as <paramref name="code"/>.</summary>
    private PartColourGroup(int code) => Code = code;

    /// <summary>Every part group, in the order of their codes.</summary>
    internal static ImmutableArray<PartColourGroup> Every { get; } =
        [.. Enumerable.Range(0, Codes).Select(code => new PartColourGroup(code))];

    /// <summary>Whether the installed image may store <paramref name="code"/> for a pixel's part group.</summary>
    internal static bool IsCode(int code) => code is >= 0 and < Codes;

    /// <summary>The part group the installed image stores as <paramref name="code"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The code names no part group.</exception>
    internal static PartColourGroup FromCode(int code) => Every[Ensure.BetweenInclusive(0, Codes - 1, code)];

    /// <summary>The number the installed image stores the part group as.</summary>
    public int Code { get; }

    /// <summary>The colour group the part was drawn in.</summary>
    internal ImageColourGroup Group => ImageColourGroup.Numbered(Code % ImageColourGroup.Count);

    /// <summary>Whether the part kept its own colour group rather than following its spritemap's palette.</summary>
    internal bool KeepsOwnColours => Code >= ImageColourGroup.Count;
}
