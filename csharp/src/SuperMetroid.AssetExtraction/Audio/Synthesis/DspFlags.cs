using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// FLG ($6C): the reset, mute and echo-write-disable control bits sharing one register with the
/// five-bit noise clock rate. The control bits and the noise rate are separate meanings
/// of the same register byte: the control bits are bits 5-7 and the noise rate bits 0-4.
/// </summary>
/// <param name="Value">The complete FLG byte.</param>
internal readonly record struct DspFlags(int Value)
{
    /// <summary>The complete FLG byte, 0 through 255; the register file stores it at its own width.</summary>
    public int Value { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Value);

    /// <summary>The FLG byte the driver's flag copy holds; a value outside a byte is a managed-player defect.</summary>
    public static DspFlags FromDriverByte(int value) => new(checked((byte)value));

    /// <summary>The noise rate an instrument's flag byte selects (its low five bits).</summary>
    public static int NoiseRateOf(int instrumentFlags) => instrumentFlags & SnesDspRegisterLayout.Fields.NoiseRateMask;
}
