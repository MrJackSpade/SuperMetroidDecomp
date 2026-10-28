using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// Stock Kraid colors for the five native sources (room backdrop $A7:86C7, initial target
/// $A7:AAA6, health $A7:B3D3, death arm $A7:B4F3, secondary $A7:B513). Shared structure
/// calculates in <see cref="KraidPaint"/>: the white hit flash, the backdrop-blue transparent slot, the endpoint band's
/// eight-shade hide ramp, the death arm's reuse of the first health band and the secondary
/// copy of health. Health bands two through seven interpolate in <see cref="KraidHealthBands"/>.
/// Retained here as authored paint: the hull, eye and claw colors, the final health band's
/// shades, the death arm's three darkened hull colors, the backdrop and target paints and the
/// two color-six health steps the cartridge sets off the interpolation.
/// </summary>
internal static class KraidPaintDefinitions
{
    /// <summary>Largest SNES RGB5 channel value used by the hit-flash and bright paint endpoints.</summary>
    internal const int Rgb5Maximum = (1 << 5) - 1;
    /// <summary>Health palette bands, including the flash and final hide endpoints.</summary>
    internal const int Bands = KraidPalettes.HealthBandCount;
    /// <summary>Colors per Kraid health band for flattened index decoding.</summary>
    internal const int BandColors = KraidPalettes.BandColors;

    /// <summary>First and last colors of the first health band's hide ramp (colors four to eleven).</summary>
    internal static readonly RgbColor RampLight = new(26, 19, 4);
    /// <summary>Dark endpoint of the first health band's eight-shade hide ramp.</summary>
    internal static readonly RgbColor RampDark = new(3, 1, 0);
    /// <summary>First slot of the first health band's calculated hide ramp.</summary>
    internal const int RampFirst = 4;
    /// <summary>Last slot of that ramp before independently chosen eye and claw inks.</summary>
    internal const int RampLast = 11;

    /// <summary>The white of the hit flash and the brightest paints.</summary>
    internal static readonly RgbColor White = new(Rgb5Maximum, Rgb5Maximum, Rgb5Maximum);

    /// <summary>The transparent slot after the first normal band: Kraid's room backdrop blue.</summary>
    internal static readonly RgbColor BackdropBlue = new(0, 0, 14);

    /// <summary>Hull colors one to three, unchanged across every normal health band.</summary>
    internal static readonly Dictionary<int, RgbColor> Hull = new()
    {
        [1] = new(29, 12, 21),
        [2] = new(22, 0, 6),
        [3] = new(13, 0, 4),
    };

    /// <summary>The first normal band's colours off its hide ramp and hull: the transparent and eye and claw inks.</summary>
    internal static readonly Dictionary<int, RgbColor> FirstBandInks = new()
    {
        [0] = RgbColor.Black,
        [12] = new(3, 2, 1),
        [13] = new(18, 18, 15),
        [14] = new(21, 22, 18),
        [15] = RgbColor.Black,
    };

    /// <summary>The painted final health band's warmer shade, eye, claw, and transparent colors; its hull is shared.</summary>
    internal static readonly Dictionary<int, RgbColor> FinalBandInks = new()
    {
        [0] = BackdropBlue,
        [4] = new(31, 28, 18),
        [5] = new(23, 25, 15),
        [6] = new(16, 22, 13),
        [7] = new(9, 19, 11),
        [8] = new(8, 16, 9),
        [9] = new(6, 13, 7),
        [10] = new(5, 9, 4),
        [11] = new(5, 6, 2),
        [12] = new(3, 0, 0),
        [13] = new(24, 24, 24),
        [14] = White,
        [15] = RgbColor.Black,
    };

    /// <summary>The two color-six health steps the cartridge sets off the interpolation, by band and color.</summary>
    internal static readonly Dictionary<KraidPaintCell, RgbColor> HealthSteps = new()
    {
        [new(5, 6)] = new(17, 20, 10),
        [new(6, 6)] = new(16, 21, 12),
    };

    /// <summary>The death arm's backdrop-blue slot and darkened hull; its other colors are the first health band's.</summary>
    internal static readonly Dictionary<int, RgbColor> DeathArmInks = new()
    {
        [0] = BackdropBlue,
        [1] = new(7, 0, 2),
        [2] = new(4, 0, 1),
        [3] = RgbColor.Black,
    };

    /// <summary>The authored room-backdrop greys, greens, and yellow accents, by color.</summary>
    internal static readonly RgbColor[] RoomBackdrop =
    [
        new(0, 0, 1), new(11, 11, 11), new(11, 11, 11), new(11, 11, 11),
        new(6, 12, 6), new(2, 6, 5), new(0, 2, 2), new(0, 0, 1),
        new(13, 15, 12), new(6, 6, 6), new(3, 6, 4), new(0, 1, 1),
        new(18, 18, 0), new(11, 11, 11), new(18, 18, 18), RgbColor.Black,
    ];

    /// <summary>The initial target band, by color: only colors five to eight are painted.</summary>
    internal static readonly RgbColor[] InitialTarget =
    [
        RgbColor.Black, RgbColor.Black, RgbColor.Black, RgbColor.Black,
        RgbColor.Black, new(22, 15, 3), new(18, 12, 3), new(12, 7, 3),
        new(9, 5, 3), RgbColor.Black, RgbColor.Black, RgbColor.Black,
        RgbColor.Black, RgbColor.Black, RgbColor.Black, RgbColor.Black,
    ];
}
