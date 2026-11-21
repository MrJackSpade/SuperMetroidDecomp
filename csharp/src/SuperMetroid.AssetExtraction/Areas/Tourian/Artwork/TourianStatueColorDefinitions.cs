using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

/// <summary>The shape of the Tourian statue's installed colours (#1415).</summary>
public static class TourianStatueColorDefinitions
{

    /// <summary>Each boss statue's eye glow shade, keyed by the statue it belongs to (#1747, #2482, #2565).</summary>
    public static IReadOnlyDictionary<SuperMetroid.Core.Areas.Tourian.TourianStatue, PictureShade> EyeGlows { get; } =
        new Dictionary<SuperMetroid.Core.Areas.Tourian.TourianStatue, PictureShade>
        {
            [SuperMetroid.Core.Areas.Tourian.TourianStatue.Phantoon] = TourianStatueShades.PhantoonEyeGlow,
            [SuperMetroid.Core.Areas.Tourian.TourianStatue.Ridley] = TourianStatueShades.RidleyEyeGlow,
            [SuperMetroid.Core.Areas.Tourian.TourianStatue.Draygon] = TourianStatueShades.DraygonEyeGlow,
            [SuperMetroid.Core.Areas.Tourian.TourianStatue.Kraid] = TourianStatueShades.KraidEyeGlow,
        };

    /// <summary>The order the statue sheets list the boss statues' eye-glow shades in (#2567).</summary>
    public static IReadOnlyList<SuperMetroid.Core.Areas.Tourian.TourianStatue> EyeGlowOrder { get; } =
    [
        SuperMetroid.Core.Areas.Tourian.TourianStatue.Phantoon,
        SuperMetroid.Core.Areas.Tourian.TourianStatue.Ridley,
        SuperMetroid.Core.Areas.Tourian.TourianStatue.Draygon,
        SuperMetroid.Core.Areas.Tourian.TourianStatue.Kraid,
    ];

    /// <summary>The colours of one eye glow.</summary>
    public const int EyeColorCount = 4;

    /// <summary>The eight grey colours a released statue turns; its animated tiles choose which colours they replace.</summary>
    public const int GreyColorCount = 8;
}
