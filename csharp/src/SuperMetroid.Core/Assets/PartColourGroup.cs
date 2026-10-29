using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colour group of the part a pixel of an installed image belongs to, and whether that part keeps its group's colours
/// under a drawn colouring or follows the colouring (#1464). A following part shows what its draw's placement combines
/// the colouring with its group into (<see cref="SpriteColourPlacement"/>); a kept part, which the import pipeline makes
/// of a part naming its own colours in a family whose parts keep them and of every background pixel, shows its own group.
/// </summary>
/// <remarks>
/// The installed image stores each pixel's part group as its <see cref="Code"/>: the group's place, raised by
/// <see cref="ImageColourGroup.Count"/> where the part keeps its colours. Backends key a recoloured draw's colours by it.
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

    /// <summary>A part of colour group <paramref name="group"/> that follows its draw's colouring.</summary>
    internal static PartColourGroup Following(ImageColourGroup group) => Every[group.Number];

    /// <summary>A part of colour group <paramref name="group"/> that keeps its own colours under any drawn colouring.</summary>
    internal static PartColourGroup Keeping(ImageColourGroup group) => Every[group.Number + ImageColourGroup.Count];

    /// <summary>Whether the installed image may store <paramref name="code"/> for a pixel's part group.</summary>
    internal static bool IsCode(int code) => code is >= 0 and < Codes;

    /// <summary>The part group the installed image stores as <paramref name="code"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The code names no part group.</exception>
    internal static PartColourGroup FromCode(int code) => Every[Ensure.BetweenInclusive(0, Codes - 1, code)];

    /// <summary>The number the installed image stores the part group as, and backends key its colours by.</summary>
    public int Code { get; }

    /// <summary>The colour group the part was drawn in.</summary>
    internal ImageColourGroup Group => ImageColourGroup.Numbered(Code % ImageColourGroup.Count);

    /// <summary>Whether the part keeps its group's colours under a drawn colouring.</summary>
    internal bool KeepsOwnColours => Code >= ImageColourGroup.Count;
}
