using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.Kraid.Artwork.KraidPaintDefinitions;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// Paints Kraid's stock colors for the five native sources from the authored inks in
/// <see cref="KraidPaintDefinitions"/>: the hit flash, the first band's hide ramp, the death arm's reuse of the first
/// band and the secondary copy of health calculate here.
/// </summary>
internal static class KraidPaint
{
    /// <summary>Routes room, target, health, death-arm, and secondary palette requests to their stock paint rules.</summary>
    internal static RgbColor Color(KraidPaletteSource source, int index)
    {
        if ((uint)index >= KraidPalettes.ColorCount(source))
            throw new ArgumentOutOfRangeException(nameof(index));
        return source switch
        {
            KraidPaletteSource.RoomBackdrop => RoomBackdrop[index],
            KraidPaletteSource.InitialTarget => InitialTarget[index],
            KraidPaletteSource.Health or KraidPaletteSource.Secondary => Health(index),
            KraidPaletteSource.DeathArm => DeathArmInks.TryGetValue(index, out RgbColor ink) ? ink : FirstBand(index),
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };
    }

    /// <summary>
    /// Stock health colors: the flash band, both endpoint bands, and the two authored
    /// color-six steps. Other interior bands are interpolated and return <see langword="null"/> here.
    /// </summary>
    internal static RgbColor? HealthAuthored(int index)
    {
        int band = index / BandColors;
        int color = index % BandColors;
        if (band == 0) return White;
        if (band == 1) return FirstBand(color);
        if (band == Bands - 1) return FinalBand(color);
        return HealthSteps.TryGetValue(new KraidPaintCell(band, color), out RgbColor step) ? step : null;
    }

    /// <summary>Uses authored endpoints and off-ramp steps, interpolating other health-band colors.</summary>
    private static RgbColor Health(int index)
    {
        if (HealthAuthored(index) is RgbColor authored) return authored;
        int band = index / BandColors;
        int color = index % BandColors;
        return color == 0 ? BackdropBlue
            : KraidHealthBands.Interpolate(FirstBand(color), FinalBand(color), band);
    }

    /// <summary>Builds the first normal band from hull inks and the nearest-rounded hide ramp.</summary>
    private static RgbColor FirstBand(int color)
    {
        if (color is >= RampFirst and <= RampLast)
        {
            int step = color - RampFirst;
            const int steps = RampLast - RampFirst;
            return new RgbColor(Nearest(RampLight.Red, RampDark.Red, step, steps),
                Nearest(RampLight.Green, RampDark.Green, step, steps),
                Nearest(RampLight.Blue, RampDark.Blue, step, steps));
        }
        return FirstBandInks.TryGetValue(color, out RgbColor ink) ? ink : HullColor(color);
    }

    /// <summary>Supplies the painted final health band's colors over the shared hull.</summary>
    private static RgbColor FinalBand(int color) => FinalBandInks.TryGetValue(color, out RgbColor ink) ? ink : HullColor(color);

    /// <summary>Hull colors one to three, unchanged across every normal health band.</summary>
    private static RgbColor HullColor(int color) =>
        Hull.TryGetValue(color, out RgbColor hull) ? hull : throw new ArgumentOutOfRangeException(nameof(color));

    /// <summary>Rounds an RGB channel to the nearest step between two painted ramp endpoints.</summary>
    private static int Nearest(int start, int end, int step, int steps)
    {
        int delta = end - start;
        return start + Math.Sign(delta) * ((Math.Abs(delta) * step + steps / 2) / steps);
    }
}
