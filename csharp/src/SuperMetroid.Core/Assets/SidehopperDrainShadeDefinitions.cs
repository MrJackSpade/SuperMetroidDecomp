namespace SuperMetroid.Core.Assets;

/// <summary>Exact RGB5 shade relationships in the final Sidehopper drain image at $A9:EC6C.</summary>
internal static class SidehopperDrainShadeDefinitions
{
    /// <summary>$A9:EC6E-EC74: first shade ramp, copied to visible inks2..5; dark matches corpse endpoint ECAA/F8C4.</summary>
    internal const int FirstColor = 1;
    /// <summary>$A9:EC76-EC7C: second shade ramp, copied to visible inks6..9.</summary>
    internal const int LastColor = 8;
    /// <summary>$A9:EC70/EC78: both upper intermediate reds are seven below their own light endpoint (31→24 and27→20). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    private const int SelectedLightRedDrop = 7;
    /// <summary>$A9:EC72/EC7A: both lower intermediate reds are four above their own dark endpoint (5→9 and12→16). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    private const int SelectedDarkRedLift = 4;

    private static int LightShadeRed(ushort light) => (light & 31) - SelectedLightRedDrop;
    private static int DarkShadeRed(ushort dark) => (dark & 31) + SelectedDarkRedLift;

    internal static bool Matches(ReadOnlySpan<ushort> colors) =>
        MatchesShade(colors[1], colors[4], LightShadeRed(colors[1]), colors[2]) &&
        MatchesShade(colors[1], colors[4], DarkShadeRed(colors[4]), colors[3]) &&
        MatchesShade(colors[5], colors[8], LightShadeRed(colors[5]), colors[6]) &&
        MatchesShade(colors[5], colors[8], DarkShadeRed(colors[8]), colors[7]);

    /// <summary>Resolves the two matched ramps from three independently supplied paints and the shared corpse dark.</summary>
    internal static ushort Resolve(int color, ReadOnlySpan<ushort> anchors, ushort bodyDark) =>
        Resolve(color, anchors[0], anchors[1], anchors[2], bodyDark);

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

    private static bool MatchesShade(ushort light, ushort dark, int red, ushort supplied)
    {
        int lightRed = light & 31, darkRed = dark & 31;
        return lightRed > darkRed && red >= darkRed && red <= lightRed && Shade(light, dark, red) == supplied;
    }

    private static ushort Shade(ushort light, ushort dark, int red)
    {
        int lightRed = light & 31, darkRed = dark & 31;
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((light >> shift & 31) * (red - darkRed) + (dark >> shift & 31) * (lightRed - red)) / (lightRed - darkRed)) << shift;
        return (ushort)result;
    }
}
