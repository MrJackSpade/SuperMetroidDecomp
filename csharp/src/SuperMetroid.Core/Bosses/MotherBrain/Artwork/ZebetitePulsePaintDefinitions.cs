namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Selected red barrier-core paint at $A6:FD87-FDA6, copied to OBJ palette2 inksC/D
/// by $A6:FD7C-82. Native $A6:FE08 and the Rinka sheet $AE:B800 use these inks for
/// adjacent bright/dark core facets. Exact hue, shade ratio and symmetric pulse
/// are chosen illumination content; no health, collision or cadence is defined here.
/// </summary>
internal static class ZebetitePulsePaintDefinitions
{
    /// <summary>Upper bound of each SNES five-bit color channel.</summary>
    internal const int ChannelMaximum = 31;
    /// <summary>$A6:FD87/$A2:BA73: warm-green bias2 on the saturated-red core.
    /// The neighboring darker facet uses half this green bias.</summary>
    internal const int WarmGreenBias = 2;
    /// <summary>$A6:FD89/$A2:BA75: the darker facet begins at floor(3/4 of red31)=23.
    /// Three-quarter shade is the selected material contrast, not a health ratio.</summary>
    internal const int DarkShadeNumerator = 3;
    /// <summary>Denominator of the darker facet's three-quarter initial red shade.</summary>
    internal const int DarkShadeDenominator = 4;
}
