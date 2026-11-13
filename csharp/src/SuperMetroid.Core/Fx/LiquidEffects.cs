using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>The waveforms liquid surfaces and heat haze follow, and lava's choice of bubbling sound (#1415).</summary>
public static class LiquidEffects
{
    /// <summary>One period of the water wave, sixteen samples (#1468): the waveform a water surface's waves and the
    /// vertical heat haze are drawn from.</summary>
    public static ImmutableArray<int> WaterWaveform { get; } =
        [.. Enumerable.Range(0, WaterWaveDefinitions.WaveDisplacementCount).Select(index => WaterWaveDisplacement(index))];

    /// <summary>One period of the horizontal heat haze, sixteen samples (#1468).</summary>
    public static ImmutableArray<int> HeatHorizontalWaveform { get; } =
        [.. Enumerable.Range(0, LavaAcidWaveDefinitions.WaveDisplacementCount).Select(index => HeatHorizontalWaveDisplacement(index))];

    /// <summary>Calculates the signed row displacement for a water wave index 0..15.</summary>
    /// <remarks>The water waveform consists of two identical eight-sample
    /// periods. Within each four-sample half-wave, reflect the integer ramp around
    /// its midpoint: min(index&amp;3, 3-(index&amp;3)). Bit two reverses its sign.
    /// This is an exact integer triangular waveform, without a floating-point
    /// generation claim. Water background/effect overlay and the identical vertical lava/acid wave
    /// share this mapping.
    /// An index outside the sixteen samples is rejected before periodic folding.</remarks>
    public static int WaterWaveDisplacement(int index)
    {
        if ((uint)index >= WaterWaveDefinitions.WaveDisplacementCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int position = index & 3;
        int magnitude = Math.Min(position, 3 - position);
        return (index & 4) == 0 ? magnitude : -magnitude;
    }

    /// <summary>
    /// The background's signed horizontal scroll displacement for index 0..15, the lava/acid heat haze.
    /// This pulse pair subtracts a four-sample unit pulse starting at eight
    /// from the same pulse starting at two. The haze
    /// rotates all sixteen samples; its two unequal zero gaps are preserved.
    /// No sinusoidal interpolation or extrapolation is implied.
    /// </summary>
    public static int HeatHorizontalWaveDisplacement(int index)
    {
        if ((uint)index >= LavaAcidWaveDefinitions.WaveDisplacementCount) throw new ArgumentOutOfRangeException(nameof(index));
        int positivePulse = (uint)(index - 2) < 4 ? 1 : 0;
        int negativePulse = (uint)(index - 8) < 4 ? 1 : 0;
        return positivePulse - negativePulse;
    }

    /// <summary>
    /// The lava sounds: enemy-library sounds chosen by the low three
    /// RNG bits.
    /// </summary>
    public static SoundEffectId LavaAmbientSound(int randomIndex) => LavaAcidWaveDefinitions.AmbientSoundChoice.Of(randomIndex) switch
    {
        0 or 3 or 6 => EnemySound.LavaBubbling1,
        1 or 4 or 7 => EnemySound.LavaBubbling2,
        _ => EnemySound.LavaBubbling3,
    };
}
