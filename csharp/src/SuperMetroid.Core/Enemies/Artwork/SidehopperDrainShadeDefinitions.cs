using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Exact RGB5 shade relationships in the final Sidehopper drain image at $A9:EC6C.</summary>
internal static class SidehopperDrainShadeDefinitions
{
    /// <summary>$A9:EC70/EC78: both upper intermediate reds are seven below their own light endpoint (31→24 and27→20). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    private const int SelectedLightRedDrop = 7;
    /// <summary>$A9:EC72/EC7A: both lower intermediate reds are four above their own dark endpoint (5→9 and12→16). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    private const int SelectedDarkRedLift = 4;

    /// <summary>Places an upper intermediate red seven steps below its light endpoint.</summary>
    private static int LightShadeRed(RgbColor light) => light.Red - SelectedLightRedDrop;
    /// <summary>Places a lower intermediate red four steps above its dark endpoint.</summary>
    private static int DarkShadeRed(RgbColor dark) => dark.Red + SelectedDarkRedLift;

    /// <summary>Selects the eight body/detail endpoints and intermediate shades in native palette order.</summary>
    internal static RgbColor Resolve(int color, RgbColor bodyLight, RgbColor detailLight, RgbColor detailDark, RgbColor bodyDark) => color switch
    {
        1 => bodyLight,
        2 => Shade(bodyLight, bodyDark, LightShadeRed(bodyLight)),
        3 => Shade(bodyLight, bodyDark, DarkShadeRed(bodyDark)),
        4 => bodyDark,
        5 => detailLight,
        6 => Shade(detailLight, detailDark, LightShadeRed(detailLight)),
        7 => Shade(detailLight, detailDark, DarkShadeRed(detailDark)),
        8 => detailDark,
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    /// <summary>Interpolates each RGB5 channel between its endpoints at the chosen red-channel fraction.</summary>
    private static RgbColor Shade(RgbColor light, RgbColor dark, int red)
    {
        int lightRed = light.Red, darkRed = dark.Red;
        return light.Zip(dark, (_, a, b) => ((a * (red - darkRed) + b * (lightRed - red)) / (lightRed - darkRed)));
    }
}
