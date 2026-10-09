namespace SuperMetroid.Core.Assets;

/// <summary>Exact RGB5 shade relationships in the final Sidehopper drain image at $A9:EC6C.</summary>
internal static class SidehopperDrainShadeDefinitions
{
    /// <summary>$A9:EC70/EC78: both upper intermediate reds are seven below their own light endpoint (31→24 and27→20). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    private const int SelectedLightRedDrop = 7;
    /// <summary>$A9:EC72/EC7A: both lower intermediate reds are four above their own dark endpoint (5→9 and12→16). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    private const int SelectedDarkRedLift = 4;

    /// <summary>Chooses the red component for the upper intermediate shade, seven levels below its light endpoint.</summary>
    /// <param name="light">Light RGB5 endpoint whose red channel anchors the reviewed seven-level drop.</param>
    /// <returns>The endpoint red value reduced by <see cref="SelectedLightRedDrop"/>.</returns>
    private static int LightShadeRed(ushort light) => (light & 31) - SelectedLightRedDrop;

    /// <summary>Chooses the red component for the lower intermediate shade, four levels above its dark endpoint.</summary>
    /// <param name="dark">Dark RGB5 endpoint whose red channel anchors the reviewed four-level lift.</param>
    /// <returns>The endpoint red value increased by <see cref="SelectedDarkRedLift"/>.</returns>
    private static int DarkShadeRed(ushort dark) => (dark & 31) + SelectedDarkRedLift;

    /// <summary>Resolves one drain-image palette entry from its body or detail light/dark endpoints.</summary>
    /// <param name="color">Palette index from one through eight; other values are rejected.</param>
    /// <param name="bodyLight">Light endpoint used by the body-color pair.</param>
    /// <param name="detailLight">Light endpoint used by the detail-color pair.</param>
    /// <param name="detailDark">Dark endpoint used by the detail-color pair.</param>
    /// <param name="bodyDark">Dark endpoint used by the body-color pair.</param>
    /// <returns>The selected endpoint or its authored intermediate RGB5 shade.</returns>
    internal static ushort Resolve(int color, ushort bodyLight, ushort detailLight, ushort detailDark, ushort bodyDark) => color switch
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

    /// <summary>Interpolates all RGB5 channels between two endpoints using a selected red-channel value as the ratio.</summary>
    /// <param name="light">Lighter RGB5 endpoint for the interpolation.</param>
    /// <param name="dark">Darker RGB5 endpoint for the interpolation.</param>
    /// <param name="red">Red-channel value that sets the position between the endpoint reds.</param>
    /// <returns>An RGB5 color whose channels follow the endpoint interpolation at <paramref name="red"/>.</returns>
    private static ushort Shade(ushort light, ushort dark, int red)
    {
        int lightRed = light & 31, darkRed = dark & 31;
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((light >> shift & 31) * (red - darkRed) + (dark >> shift & 31) * (lightRed - red)) / (lightRed - darkRed)) << shift;
        return (ushort)result;
    }
}
