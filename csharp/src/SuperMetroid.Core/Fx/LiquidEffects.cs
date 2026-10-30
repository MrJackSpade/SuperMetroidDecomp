using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>The waveforms liquid surfaces and heat haze follow, and lava's choice of bubbling sound (#1415).</summary>
public static class LiquidEffects
{
    /// <summary>One period of the water wave, sixteen samples (#1468): the waveform a water surface's waves and the
    /// vertical heat haze are drawn from.</summary>
    public static ImmutableArray<int> WaterWaveform { get; } =
        [.. Enumerable.Range(0, WaterWaveDefinitions.WaveDisplacementCount).Select(index => (int)WaterWaveDisplacement(index))];

    /// <summary>One period of the horizontal heat haze, sixteen samples (#1468).</summary>
    public static ImmutableArray<int> HeatHorizontalWaveform { get; } =
        [.. Enumerable.Range(0, LavaAcidWaveDefinitions.WaveDisplacementCount).Select(index => (int)HeatHorizontalWaveDisplacement(index))];

    /// <summary>Calculates the signed scanline displacement for a water wave index 0..15.</summary>
    /// <remarks>The original $88:C46E waveform consists of two identical eight-sample
    /// periods. Within each four-sample half-wave, reflect the integer ramp around
    /// its midpoint: min(index&amp;3, 3-(index&amp;3)). Bit two reverses its sign.
    /// This is an exact integer triangular waveform, without a floating-point
    /// generation claim. Water BG2/BG3 and the identical vertical lava/acid wave
    /// share this mapping. All original words at C46E and B60A are independently
    /// checked against supported NTSC J/U v1.0 and pinned bank_88.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// Reject outside the original sixteen-word span before periodic folding.</remarks>
    public static short WaterWaveDisplacement(int index)
    {
        if ((uint)index >= WaterWaveDefinitions.WaveDisplacementCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int position = index & 3;
        int magnitude = Math.Min(position, 3 - position);
        return (short)((index & 4) == 0 ? magnitude : -magnitude);
    }

    /// <summary>
    /// Signed BG2HOFS displacement for index 0..15, matching $88:B589.
    /// This pulse pair subtracts a four-sample unit pulse starting at eight
    /// from the same pulse starting at two. The native $88:B53B consumer
    /// rotates all sixteen samples; its two unequal zero gaps are preserved.
    /// No sinusoidal interpolation or extrapolation is implied.
    /// </summary>
    public static short HeatHorizontalWaveDisplacement(int index)
    {
        if ((uint)index >= LavaAcidWaveDefinitions.WaveDisplacementCount) throw new ArgumentOutOfRangeException(nameof(index));
        int positivePulse = (uint)(index - 2) < 4 ? 1 : 0;
        int negativePulse = (uint)(index - 8) < 4 ? 1 : 0;
        return (short)(positivePulse - negativePulse);
    }

    /// <summary>
    /// <c>Lava_SoundEffects</c> ($88:B3A1): library-two sounds chosen by the low three
    /// RNG bits.
    /// </summary>
    public static SoundEffectId LavaAmbientSound(int randomIndex) => (randomIndex & 7) switch
    {
        0 or 3 or 6 => Library2Sound.LavaBubbling1,
        1 or 4 or 7 => Library2Sound.LavaBubbling2,
        _ => Library2Sound.LavaBubbling3,
    };
}
