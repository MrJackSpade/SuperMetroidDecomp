using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Frontend.Title.Artwork.TitleAmbientColorDefinitions;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// Calculates the title screen's ambient palette colours (<see cref="TitleAmbientColorDefinitions"/>) from the initial palette.
/// </summary>
internal static class TitleAmbientColors
{
    /// <summary>Derives a title tube shade or display paint from the selected initial palette and any supplied first-frame edit.</summary>
    internal static bool TryCalculate(PaletteFxColorKey key, ReadOnlySpan<RgbColor> initial,
        IReadOnlyDictionary<PaletteFxColorKey, RgbColor> supplied, out RgbColor color)
    {
        var tube = TitleScreenAmbientPaletteFxColorDefinitions.TubeLight;
        if (tube.Rows.TryLocate(key, out int frame, out int index))
        {
            RgbColor first = ScreenColourGroups.In(initial, ScreenColourGroup.TitleTubeLight)[index];
            if (frame == 0) { color = first; return true; }
            if (supplied.TryGetValue(tube.Color(0, index), out RgbColor edited)) first = edited;
            int phase = Math.Min(frame, tube.FrameCount - frame);
            color = first.Map((_, channel) => Channel(channel, phase));
            return true;
        }
        var displays = TitleScreenAmbientPaletteFxColorDefinitions.Displays;
        if (displays.Rows.TryLocate(key, out frame, out index))
        {
            color = frame == 0 ? ScreenColourGroups.In(initial, ScreenColourGroup.TitleDisplays)[index]
                : ScreenColourGroups.ColourIn(initial, index == 0 ? ScreenColourGroup.TitleDimWarmPaint : ScreenColourGroup.TitleDimGreenPaint);
            return true;
        }
        color = RgbColor.Black;
        return false;
    }

    /// <summary>Dims one RGB5 channel by the reviewed three-unit scale with exact nearest-even integer rounding.</summary>
    private static int Channel(int first, int phase)
    {
        int steps = Math.Min(TitleScreenAmbientPaletteFxColorDefinitions.TubeDimmingSteps,
            (first + ShadeStep - 1) / ShadeStep);
        int endpoint = Math.Max(0, first - ShadeStep * steps);
        if (phase >= steps) return endpoint;
        int numerator = first * steps - (first - endpoint) * phase;
        int quotient = Math.DivRem(numerator, steps, out int remainder);
        // Exact rational nearest/even quantization: no floating-point contraction or rounding drift.
        return remainder * 2 > steps || (remainder * 2 == steps && (quotient & 1) != 0)
            ? quotient + 1 : quotient;
    }
}
