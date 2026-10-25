using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Title tube shading and display paint bindings at $8D:C800-C873.</summary>
/// <remarks>The initial palette remains the independent paint owner. The selected three-unit
/// shade scale, rounded channel interpolation and dim-paint roles describe this animation;
/// no isolated green-channel exception or duplicate stock endpoint is stored.</remarks>
internal static class TitleAmbientColorDefinitions
{
    /// <summary>$8D:C800-C85B: nominal three-unit RGB5 dimming step, with shorter channels distributed to black.</summary>
    private const int ShadeStep = 3;
    /// <summary>$8D:C870: dim warm display paint reuses title palette slot3 ($8C:E1EF).</summary>
    private const int DimWarmPaint = 3;
    /// <summary>$8D:C872: dim green display paint reuses title palette slot19 ($8C:E20F).</summary>
    private const int DimGreenPaint = 19;

    /// <summary>Derives a title tube shade or display paint from the selected initial palette and any supplied first-frame edit.</summary>
    internal static bool TryCalculate(PaletteFxColorKey key, ReadOnlySpan<RgbColor> initial,
        IReadOnlyDictionary<PaletteFxColorKey, RgbColor> supplied, out RgbColor color)
    {
        var tube = TitleScreenAmbientPaletteFxColorDefinitions.TubeLight;
        if (tube.Rows.TryLocate(key, out int frame, out int index))
        {
            RgbColor first = initial[tube.ColorByteIndex / RgbColor.ByteCount + index];
            if (frame == 0) { color = first; return true; }
            if (supplied.TryGetValue(tube.Color(0, index), out RgbColor edited)) first = edited;
            int phase = Math.Min(frame, tube.FrameCount - frame);
            color = first.Map((_, channel) => Channel(channel, phase));
            return true;
        }
        var displays = TitleScreenAmbientPaletteFxColorDefinitions.Displays;
        if (displays.Rows.TryLocate(key, out frame, out index))
        {
            int paletteIndex = frame == 0 ? displays.ColorByteIndex / RgbColor.ByteCount + index
                : index == 0 ? DimWarmPaint : DimGreenPaint;
            color = initial[paletteIndex];
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
