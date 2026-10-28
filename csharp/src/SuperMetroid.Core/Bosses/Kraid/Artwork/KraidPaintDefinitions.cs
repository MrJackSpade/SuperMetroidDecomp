using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// Stock Kraid colors for the five native sources (room backdrop $A7:86C7, initial target
/// $A7:AAA6, health $A7:B3D3, death arm $A7:B4F3, secondary $A7:B513). Shared structure
/// calculates: the white hit flash, the backdrop-blue transparent slot, the endpoint band's
/// eight-shade hide ramp, the death arm's reuse of the first health band and the secondary
/// copy of health. Health bands two through seven interpolate in <see cref="KraidColorCatalog"/>.
/// Retained as authored paint: the hull, eye and claw colors, the final health band's
/// shades, the death arm's three darkened hull colors, the backdrop and target paints and the
/// two color-six health steps the cartridge sets off the interpolation.
/// </summary>
internal static class KraidPaintDefinitions
{
    /// <summary>Largest SNES RGB5 channel value used by the hit-flash and bright paint endpoints.</summary>
    private const int Rgb5Maximum = (1 << 5) - 1;
    /// <summary>Health palette bands, including the flash and final hide endpoints.</summary>
    private const int Bands = KraidPalettes.HealthBandCount;
    /// <summary>Colors per Kraid health band for flattened index decoding.</summary>
    private const int BandColors = KraidPalettes.BandColors;

    /// <summary>First and last colors of the first health band's hide ramp (colors four to eleven).</summary>
    private static readonly RgbColor RampLight = new(26, 19, 4);
    /// <summary>Dark endpoint of the first health band's eight-shade hide ramp.</summary>
    private static readonly RgbColor RampDark = new(3, 1, 0);
    /// <summary>First slot of the first health band's calculated hide ramp.</summary>
    private const int RampFirst = 4;
    /// <summary>Last slot of that ramp before independently chosen eye and claw inks.</summary>
    private const int RampLast = 11;

    /// <summary>The transparent slot after the first normal band: Kraid's room backdrop blue.</summary>
    private static RgbColor BackdropBlue => Pack(0, 0, 14);

    /// <summary>Routes room, target, health, death-arm, and secondary palette requests to their stock paint rules.</summary>
    internal static RgbColor Color(KraidPaletteSource source, int index)
    {
        if ((uint)index >= KraidPalettes.ColorCount(source))
            throw new ArgumentOutOfRangeException(nameof(index));
        return source switch
        {
            KraidPaletteSource.RoomBackdrop => RoomBackdrop(index),
            KraidPaletteSource.InitialTarget => InitialTarget(index),
            KraidPaletteSource.Health or KraidPaletteSource.Secondary => Health(index),
            KraidPaletteSource.DeathArm => DeathArm(index),
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };
    }

    /// <summary>
    /// Stock health colors: the flash band, both endpoint bands, and the two authored
    /// color-six steps. Other interior bands are interpolated by the catalog and return
    /// <see langword="null"/> here.
    /// </summary>
    internal static RgbColor? HealthAuthored(int index)
    {
        int band = index / BandColors;
        int color = index % BandColors;
        if (band == 0) return Pack(Rgb5Maximum, Rgb5Maximum, Rgb5Maximum);
        if (band == 1) return FirstBand(color);
        if (band == Bands - 1) return FinalBand(color);
        return new KraidPaintCell(band, color) switch
        {
            (5, 6) => Pack(17, 20, 10),
            (6, 6) => Pack(16, 21, 12),
            _ => null,
        };
    }

    /// <summary>Uses authored endpoints and off-ramp steps, interpolating other health-band colors.</summary>
    private static RgbColor Health(int index)
    {
        if (HealthAuthored(index) is RgbColor authored) return authored;
        int band = index / BandColors;
        int color = index % BandColors;
        return color == 0 ? BackdropBlue
            : KraidColorCatalog.InterpolateHealth(FirstBand(color), FinalBand(color), band);
    }

    /// <summary>Builds the first normal band from hull inks and the nearest-rounded hide ramp.</summary>
    private static RgbColor FirstBand(int color)
    {
        if (color is >= RampFirst and <= RampLast)
        {
            int step = color - RampFirst;
            const int steps = RampLast - RampFirst;
            return Pack(Nearest(RampLight.Red, RampDark.Red, step, steps),
                Nearest(RampLight.Green, RampDark.Green, step, steps),
                Nearest(RampLight.Blue, RampDark.Blue, step, steps));
        }
        return color switch
        {
            0 or 15 => RgbColor.Black,
            12 => Pack(3, 2, 1),
            13 => Pack(18, 18, 15),
            14 => Pack(21, 22, 18),
            _ => Hull(color),
        };
    }

    /// <summary>Supplies the painted final health band's warmer shade, eye, claw, and transparent colors.</summary>
    private static RgbColor FinalBand(int color) => color switch
    {
        0 => BackdropBlue,
        4 => Pack(31, 28, 18),
        5 => Pack(23, 25, 15),
        6 => Pack(16, 22, 13),
        7 => Pack(9, 19, 11),
        8 => Pack(8, 16, 9),
        9 => Pack(6, 13, 7),
        10 => Pack(5, 9, 4),
        11 => Pack(5, 6, 2),
        12 => Pack(3, 0, 0),
        13 => Pack(24, 24, 24),
        14 => Pack(Rgb5Maximum, Rgb5Maximum, Rgb5Maximum),
        15 => RgbColor.Black,
        _ => Hull(color),
    };

    /// <summary>Hull colors one to three, unchanged across every normal health band.</summary>
    private static RgbColor Hull(int color) => color switch
    {
        1 => Pack(29, 12, 21),
        2 => Pack(22, 0, 6),
        3 => Pack(13, 0, 4),
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    /// <summary>The first health band with its hull darkened and the backdrop-blue slot.</summary>
    private static RgbColor DeathArm(int color) => color switch
    {
        0 => BackdropBlue,
        1 => Pack(7, 0, 2),
        2 => Pack(4, 0, 1),
        3 => RgbColor.Black,
        _ => FirstBand(color),
    };

    /// <summary>Supplies the authored room-backdrop greys, greens, and yellow accents.</summary>
    private static RgbColor RoomBackdrop(int color) => color switch
    {
        0 or 7 => Pack(0, 0, 1),
        1 or 2 or 3 or 13 => Pack(11, 11, 11),
        4 => Pack(6, 12, 6),
        5 => Pack(2, 6, 5),
        6 => Pack(0, 2, 2),
        8 => Pack(13, 15, 12),
        9 => Pack(6, 6, 6),
        10 => Pack(3, 6, 4),
        11 => Pack(0, 1, 1),
        12 => Pack(18, 18, 0),
        14 => Pack(18, 18, 18),
        _ => RgbColor.Black,
    };

    /// <summary>Only colors five to eight of the initial target band are painted.</summary>
    private static RgbColor InitialTarget(int color) => color switch
    {
        5 => Pack(22, 15, 3),
        6 => Pack(18, 12, 3),
        7 => Pack(12, 7, 3),
        8 => Pack(9, 5, 3),
        _ => RgbColor.Black,
    };

    /// <summary>Rounds an RGB channel to the nearest step between two painted ramp endpoints.</summary>
    private static int Nearest(int start, int end, int step, int steps)
    {
        int delta = end - start;
        return start + Math.Sign(delta) * ((Math.Abs(delta) * step + steps / 2) / steps);
    }

    /// <summary>Packs three validated five-bit paint components into a native CGRAM color.</summary>
    private static RgbColor Pack(int red, int green, int blue) => new RgbColor(red, green, blue);
}
