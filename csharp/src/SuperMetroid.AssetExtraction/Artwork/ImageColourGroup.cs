using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One of the cartridge's colour groups a sheet the import composes is coloured in (#1464): sixteen colours of the half
/// of the colour table the sheet names, which a composed pixel's palette index names. The installed image knows no groups;
/// it names its own colours (#1544).
/// </summary>
/// <remarks>A group is known by its place in its half of the colour table.</remarks>
public readonly record struct ImageColourGroup
{
    /// <summary>The colour groups of one half of the colour table.</summary>
    public const int Count = 8;

    /// <summary>The colours of one group.</summary>
    public const int Colours = 16;

    /// <summary>The colours of one half of the colour table, which a composed sheet names its colours from.</summary>
    public const int HalfColours = Count * Colours;

    /// <summary>Names the group at <paramref name="number"/> in its half of the colour table.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place is not one of the half's groups.</exception>
    internal ImageColourGroup(int number) => Number = Ensure.BetweenInclusive(0, Count - 1, number);

    /// <summary>The group's place in its half of the colour table.</summary>
    internal int Number { get; }

    /// <summary>The colours a sheet composed in <paramref name="colours"/> names: all 256 for a full-palette sheet.</summary>
    public static int ColourCountOf(SpriteImageColours colours) =>
        colours == SpriteImageColours.FullPalette ? 2 * HalfColours : HalfColours;
}
