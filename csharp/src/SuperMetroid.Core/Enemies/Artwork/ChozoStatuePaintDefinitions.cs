using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Stock colors for both 32-color Chozo statue palettes. Each has two sixteen-color halves.
/// Shared structure calculates: colors 0..3 and 8..13 are common to all four halves, with
/// 8..11 a nearest-integer gold ramp; Wrecked Ship colors 4..7 are nearest-integer grey ramps
/// in both halves; Lower Norfair's second-half colors 4..6 repeat its first-half 5..7, one
/// shade down. Retained as authored paint: the skin and gold endpoints, colors 14/15, Lower
/// Norfair's first-half 4..7 and second-half color 7.
/// </summary>
internal static class ChozoStatuePaintDefinitions
{
    /// <summary>Each statue palette contains two sixteen-color material halves.</summary>
    internal const int HalfColors = 16;

    /// <summary>The shades between each ramp's two endpoints.</summary>
    internal const int RampSteps = 3;

    /// <summary>The leading colors every statue half shares, by color: the transparent backdrop blue and the three skin colors.</summary>
    internal static readonly RgbColor[] SharedLeadingInks = [new(0, 0, 14), new(31, 25, 24), new(31, 20, 18), new(31, 14, 11)];

    /// <summary>The gold highlights every statue half shares after its gold ramp, from the color at <see cref="SharedGoldHighlightFirst"/>.</summary>
    internal static readonly RgbColor[] SharedGoldHighlights = [new(31, 31, 15), new(25, 25, 10)];
    /// <summary>The first shared gold highlight's color.</summary>
    internal const int SharedGoldHighlightFirst = 12;

    /// <summary>The gold ramp every statue half shares, from its brightest color at <see cref="GoldRampFirst"/>.</summary>
    internal const int GoldRampFirst = 8;
    /// <summary>The gold ramp's brightest color.</summary>
    internal static readonly RgbColor GoldRampStart = new(31, 31, 9);
    /// <summary>The gold ramp's darkest color.</summary>
    internal static readonly RgbColor GoldRampEnd = new(6, 6, 0);

    /// <summary>The Wrecked Ship statue's grey ramp, from its brightest color at <see cref="GreyRampFirst"/>.</summary>
    internal const int GreyRampFirst = 4;
    /// <summary>The first half's grey ramp's brightest color.</summary>
    internal static readonly RgbColor FirstHalfGreyStart = new(25, 25, 25);
    /// <summary>The first half's grey ramp's darkest color.</summary>
    internal static readonly RgbColor FirstHalfGreyEnd = new(6, 6, 6);
    /// <summary>The second half's grey ramp's brightest color.</summary>
    internal static readonly RgbColor SecondHalfGreyStart = new(16, 16, 16);
    /// <summary>The second half's grey ramp's darkest color.</summary>
    internal static readonly RgbColor SecondHalfGreyEnd = new(4, 4, 4);

    /// <summary>The Wrecked Ship statue's color 14 highlight in its first and second halves, and its black color 15.</summary>
    internal static readonly Dictionary<ChozoStatueShade, RgbColor> WreckedShipPaint = new()
    {
        [new(14, false)] = new(31, 31, 31),
        [new(14, true)] = new(20, 20, 20),
        [new(15, false)] = RgbColor.Black,
        [new(15, true)] = RgbColor.Black,
    };

    /// <summary>The Lower Norfair statue's authored paint; its second-half colors 4..6 repeat the first half's 5..7.</summary>
    internal static readonly Dictionary<ChozoStatueShade, RgbColor> LowerNorfairPaint = new()
    {
        [new(4, false)] = new(28, 27, 11),
        [new(5, false)] = new(21, 20, 8),
        [new(6, false)] = new(13, 12, 4),
        [new(7, false)] = new(8, 7, 2),
        [new(7, true)] = new(5, 4, 0),
        [new(14, false)] = new(31, 30, 28),
        [new(14, true)] = new(20, 20, 20),
        [new(15, false)] = new(3, 2, 0),
        [new(15, true)] = new(1, 0, 0),
    };
}
