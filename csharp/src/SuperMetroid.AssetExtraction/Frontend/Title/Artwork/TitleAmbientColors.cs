using System.Collections.Immutable;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Frontend.Title.Artwork.TitleAmbientColorDefinitions;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// Calculates the title screen's ambient palette colours (<see cref="TitleAmbientColorDefinitions"/>) from the initial
/// palette, whose colours lie where <see cref="TitleSheetInks"/> places them.
/// </summary>
internal static class TitleAmbientColors
{
    /// <summary>
    /// Derives a title tube shade or display paint from the selected initial colours <paramref name="initial"/>, in the
    /// cartridge's order, and any supplied first-frame edit.
    /// </summary>
    internal static bool TryCalculate(PaletteFxColorKey key, ImmutableArray<RgbColor> initial,
        IReadOnlyDictionary<PaletteFxColorKey, RgbColor> supplied, out RgbColor color)
    {
        var tube = TitleScreenAmbientPaletteFxColorDefinitions.TubeLight;
        if (tube.ColorFrames.TryLocate(key, out int frame, out int index))
        {
            RgbColor first = initial[TitleSheetInks.FloorLight(PaletteFxColours.BabyMetroidTubeLight).First + index];
            if (frame == 0) { color = first; return true; }
            if (supplied.TryGetValue(tube.Color(0, index), out RgbColor edited)) first = edited;
            int phase = Math.Min(frame, tube.FrameCount - frame);
            color = first.FadeLevels.Map((_, channel) => Channel(channel, phase)).ToColour();
            return true;
        }
        var displays = TitleScreenAmbientPaletteFxColorDefinitions.Displays;
        if (displays.ColorFrames.TryLocate(key, out frame, out index))
        {
            color = frame == 0 ? initial[TitleSheetInks.FloorLight(PaletteFxColours.TitleScreenDisplays).First + index]
                : initial[index == 0 ? TitleSheetInks.DimWarmPaint : TitleSheetInks.DimGreenPaint];
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
