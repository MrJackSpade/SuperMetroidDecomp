using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

/// <summary>The shape of the Tourian statue's installed colours (#1415).</summary>
public static class TourianStatueColorDefinitions
{
    /// <summary>The four boss-specific eye glows the statues step through, ordered Phantoon, Ridley, Draygon, Kraid.</summary>
    public const int EyeGlowCount = 4;

    /// <summary>Each boss statue's eye glow's row in the installed eye colours (#1747).</summary>
    public static IReadOnlyList<KeyValuePair<TourianGreyStatues, int>> EyeGlowRows { get; } =
    [
        new(TourianGreyStatues.Phantoon, 0),
        new(TourianGreyStatues.Ridley, 1),
        new(TourianGreyStatues.Draygon, 2),
        new(TourianGreyStatues.Kraid, 3),
    ];

    /// <summary>The colours of one eye glow.</summary>
    public const int EyeColorCount = 4;

    /// <summary>The eight grey colours a released statue turns; its animated tiles choose which colours they replace.</summary>
    public const int GreyColorCount = 8;
}
