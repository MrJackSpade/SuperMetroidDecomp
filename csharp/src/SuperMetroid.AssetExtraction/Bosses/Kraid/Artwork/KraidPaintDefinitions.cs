using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

/// <summary>
/// Stock Kraid colors for the five sources (room backdrop, initial target,
/// health, death arm, secondary). Shared structure
/// calculates in <see cref="KraidPaint"/>: the white hit flash, the backdrop-blue transparent slot, the endpoint band's
/// eight-shade hide ramp, the death arm's reuse of the first health band and the secondary
/// copy of health. Health bands two through seven interpolate in <see cref="KraidHealthBands"/>.
/// Retained here as authored paint: the hull, eye and claw colors, the final health band's
/// shades, the death arm's three darkened hull colors, the backdrop and target paints and the
/// two color-six health steps the original game sets off the interpolation.
/// </summary>
internal static class KraidPaintDefinitions
{
    /// <summary>Largest RGB5 channel value used by the hit-flash and bright paint endpoints.</summary>
    internal const int Rgb5Maximum = (1 << 5) - 1;
    /// <summary>Health palette bands, including the flash and final hide endpoints.</summary>
    internal const int Bands = KraidPalettes.HealthBandCount;
    /// <summary>Colors per Kraid health band for flattened index decoding.</summary>
    internal const int BandColors = KraidPalettes.BandColors;

    /// <summary>First and last colors of the first health band's hide ramp (colors four to eleven).</summary>
    internal static readonly RgbColor RampLight = new(214, 156, 33);
    /// <summary>Dark endpoint of the first health band's eight-shade hide ramp.</summary>
    internal static readonly RgbColor RampDark = new(24, 8, 0);
    /// <summary>First slot of the first health band's calculated hide ramp.</summary>
    internal const int RampFirst = 4;
    /// <summary>Last slot of that ramp before independently chosen eye and claw inks.</summary>
    internal const int RampLast = 11;

    /// <summary>The white of the hit flash and the brightest paints.</summary>
    internal static readonly RgbColor White = new FadeLevelColour(Rgb5Maximum, Rgb5Maximum, Rgb5Maximum).ToColour();

    /// <summary>The transparent slot after the first normal band: Kraid's room backdrop blue.</summary>
    internal static readonly RgbColor BackdropBlue = new(0, 0, 115);

    /// <summary>The lightest hull color, unchanged across every normal health band.</summary>
    internal static readonly RgbColor HullLight = new(239, 99, 173);
    /// <summary>The middle hull color, unchanged across every normal health band.</summary>
    internal static readonly RgbColor HullMiddle = new(181, 0, 49);
    /// <summary>The darkest hull color, unchanged across every normal health band.</summary>
    internal static readonly RgbColor HullDark = new(107, 0, 33);

    /// <summary>The first normal band's colors before its hide ramp, by color: the transparent ink and the hull.</summary>
    internal static readonly RgbColor[] FirstBandLeadingInks = [RgbColor.Black, HullLight, HullMiddle, HullDark];

    /// <summary>The first normal band's colors after its hide ramp, in order: the eye and claw inks and the outline.</summary>
    internal static readonly RgbColor[] FirstBandTrailingInks = [new(24, 16, 8), new(148, 148, 123), new(173, 181, 148), RgbColor.Black];

    /// <summary>
    /// The painted final health band, by color: the backdrop-blue transparent slot, the shared hull, then the warmer
    /// shade, eye, claw, and outline colors.
    /// </summary>
    internal static readonly RgbColor[] FinalBandInks =
    [
        BackdropBlue, HullLight, HullMiddle, HullDark,
        new(255, 231, 148), new(189, 206, 123), new(132, 181, 107), new(74, 156, 90),
        new(66, 132, 74), new(49, 107, 57), new(41, 74, 33), new(41, 49, 16),
        new(24, 0, 0), new(198, 198, 198), White, RgbColor.Black,
    ];

    /// <summary>The two color-six health steps the original game sets off the interpolation, by band and color.</summary>
    internal static readonly Dictionary<KraidPaintCell, RgbColor> HealthSteps = new()
    {
        [new(5, 6)] = new(140, 165, 82),
        [new(6, 6)] = new(132, 173, 99),
    };

    /// <summary>
    /// The death arm's leading colors, by color: the backdrop-blue slot and the darkened hull; its later colors are the
    /// first health band's.
    /// </summary>
    internal static readonly RgbColor[] DeathArmInks = [BackdropBlue, new(57, 0, 16), new(33, 0, 8), RgbColor.Black];

    /// <summary>The authored room-backdrop greys, greens, and yellow accents, by color.</summary>
    internal static readonly RgbColor[] RoomBackdrop =
    [
        new(0, 0, 8), new(90, 90, 90), new(90, 90, 90), new(90, 90, 90),
        new(49, 99, 49), new(16, 49, 41), new(0, 16, 16), new(0, 0, 8),
        new(107, 123, 99), new(49, 49, 49), new(24, 49, 33), new(0, 8, 8),
        new(148, 148, 0), new(90, 90, 90), new(148, 148, 148), RgbColor.Black,
    ];

    /// <summary>The initial target band, by color: only colors five to eight are painted.</summary>
    internal static readonly RgbColor[] InitialTarget =
    [
        RgbColor.Black, RgbColor.Black, RgbColor.Black, RgbColor.Black,
        RgbColor.Black, new(181, 123, 24), new(148, 99, 24), new(99, 57, 24),
        new(74, 41, 24), RgbColor.Black, RgbColor.Black, RgbColor.Black,
        RgbColor.Black, RgbColor.Black, RgbColor.Black, RgbColor.Black,
    ];
}
