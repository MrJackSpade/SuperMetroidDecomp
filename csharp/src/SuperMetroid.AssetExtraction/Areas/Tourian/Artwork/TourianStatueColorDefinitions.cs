using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

/// <summary>The shape of the Tourian statue's installed colours (#1415).</summary>
public static class TourianStatueColorDefinitions
{
    /// <summary>The four boss-specific eye glows the statues step through, ordered Phantoon, Ridley, Draygon, Kraid.</summary>
    public const int EyeGlowCount = 4;

    /// <summary>Each boss statue's eye glow and the statue it belongs to (#1747, #2482).</summary>
    public static IReadOnlyList<KeyValuePair<PictureShade, SuperMetroid.Core.Areas.Tourian.TourianStatue>> EyeGlowRows { get; } =
    [
        new(TourianStatueShades.PhantoonEyeGlow, SuperMetroid.Core.Areas.Tourian.TourianStatue.Phantoon),
        new(TourianStatueShades.RidleyEyeGlow, SuperMetroid.Core.Areas.Tourian.TourianStatue.Ridley),
        new(TourianStatueShades.DraygonEyeGlow, SuperMetroid.Core.Areas.Tourian.TourianStatue.Draygon),
        new(TourianStatueShades.KraidEyeGlow, SuperMetroid.Core.Areas.Tourian.TourianStatue.Kraid),
    ];

    /// <summary>The colours of one eye glow.</summary>
    public const int EyeColorCount = 4;

    /// <summary>The eight grey colours a released statue turns; its animated tiles choose which colours they replace.</summary>
    public const int GreyColorCount = 8;
}
