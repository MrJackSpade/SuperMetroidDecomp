using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.Artwork.ChozoStatuePaintDefinitions;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Paints the Chozo statues' stock colors from their shared, ramped and authored paint.</summary>
internal static class ChozoStatuePaint
{
    /// <summary>Chooses a shared shade or calculates the selected area's half-palette material paint.</summary>
    internal static RgbColor Color(ChozoStatuePalette palette, int color)
    {
        if ((uint)color >= HalfColors * 2) throw new ArgumentOutOfRangeException(nameof(color));
        int local = color % HalfColors;
        bool second = color >= HalfColors;
        if (Shared(local) is RgbColor shared) return shared;
        return palette switch
        {
            ChozoStatuePalette.WreckedShip => WreckedShip(local, second),
            ChozoStatuePalette.LowerNorfair => LowerNorfair(local, second),
            _ => throw new ArgumentOutOfRangeException(nameof(palette)),
        };
    }

    /// <summary>Calculates skin and gold colors common to all four statue halves.</summary>
    private static RgbColor? Shared(int local)
    {
        if (SharedColors.TryGetValue(local, out RgbColor shared)) return shared;
        if (local >= GoldRampFirst && local <= GoldRampFirst + RampSteps)
            return Ramp(GoldRampStart, GoldRampEnd, local - GoldRampFirst);
        return null;
    }

    /// <summary>Calculates first- and second-half grey ramps and their distinct highlight.</summary>
    private static RgbColor WreckedShip(int local, bool second)
    {
        if (local >= GreyRampFirst && local <= GreyRampFirst + RampSteps)
            return second
                ? Ramp(SecondHalfGreyStart, SecondHalfGreyEnd, local - GreyRampFirst)
                : Ramp(FirstHalfGreyStart, FirstHalfGreyEnd, local - GreyRampFirst);
        return WreckedShipPaint.TryGetValue(new ChozoStatueShade(local, second), out RgbColor paint)
            ? paint
            : throw new ArgumentOutOfRangeException(nameof(local));
    }

    /// <summary>Uses retained Lower Norfair paint, including second-half shades that repeat the first half one step darker.</summary>
    private static RgbColor LowerNorfair(int local, bool second)
    {
        if (second && local is >= 4 and <= 6) return LowerNorfair(local + 1, false);
        return LowerNorfairPaint.TryGetValue(new ChozoStatueShade(local, second), out RgbColor paint)
            ? paint
            : throw new ArgumentOutOfRangeException(nameof(local));
    }

    /// <summary>Interpolates each RGB5 channel between authored material endpoints with nearest-integer rounding.</summary>
    private static RgbColor Ramp(RgbColor start, RgbColor end, int step) =>
        new(Nearest(start.Red, end.Red, step), Nearest(start.Green, end.Green, step),
            Nearest(start.Blue, end.Blue, step));

    /// <summary>Rounds one signed shade step to its nearest channel value.</summary>
    private static int Nearest(int start, int end, int step)
    {
        int delta = end - start;
        return start + Math.Sign(delta) * ((Math.Abs(delta) * step + RampSteps / 2) / RampSteps);
    }
}
