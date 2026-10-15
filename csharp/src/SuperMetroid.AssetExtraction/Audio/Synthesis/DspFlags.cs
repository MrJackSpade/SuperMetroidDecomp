namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// FLG ($6C): the reset, mute and echo-write-disable control bits sharing one register with the
/// five-bit noise clock rate. The control bits and the noise rate are separate meanings
/// of the same register byte: the control bits are bits 5-7 and the noise rate bits 0-4.
/// </summary>
/// <param name="Value">The complete FLG byte.</param>
[DspRegisterBoundary("typed controls: FLG control bits and noise rate share one register")]
internal readonly record struct DspFlags(byte Value)
{
    /// <summary>The noise rate an instrument's flag byte selects (its low five bits).</summary>
    public static int NoiseRateOf(byte instrumentFlags) => instrumentFlags & SnesDspRegisterLayout.Fields.NoiseRateMask;
}
