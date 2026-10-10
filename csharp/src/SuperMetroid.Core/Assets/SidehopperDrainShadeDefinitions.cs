using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Exact RGB5 shade relationships in the final Sidehopper drain image at $A9:EC6C.</summary>
internal static class SidehopperDrainShadeDefinitions
{
    /// <summary>$A9:EC70/EC78: both upper intermediate reds are seven below their own light endpoint (31→24 and27→20). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    private const int SelectedLightRedDrop = 7;
    /// <summary>$A9:EC72/EC7A: both lower intermediate reds are four above their own dark endpoint (5→9 and12→16). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    private const int SelectedDarkRedLift = 4;

    private static int LightShadeRed(Bgr555 light) => light.Red - SelectedLightRedDrop;
    private static int DarkShadeRed(Bgr555 dark) => dark.Red + SelectedDarkRedLift;

    internal static Bgr555 Resolve(int color, Bgr555 bodyLight, Bgr555 detailLight, Bgr555 detailDark, Bgr555 bodyDark) => color switch
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

    private static Bgr555 Shade(Bgr555 light, Bgr555 dark, int red)
    {
        int lightRed = light.Red, darkRed = dark.Red;
        return light.Zip(dark, (_, a, b) => ((a * (red - darkRed) + b * (lightRed - red)) / (lightRed - darkRed)));
    }
}
