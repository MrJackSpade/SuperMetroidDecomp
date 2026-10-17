using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One six-byte instrument record of the driver's instrument table (#632).</summary>
/// <param name="SourceOrNoiseRate">Sample source number, or the noise rate when <see cref="SpcDriverDefinitions.Instruments.NoiseMarker"/> is set.</param>
/// <param name="Adsr1">VxADSR1 value.</param>
/// <param name="Adsr2">VxADSR2 value.</param>
/// <param name="Gain">VxGAIN value.</param>
/// <param name="PitchBase">Pitch multiplier, stored big-endian in the record.</param>
internal readonly record struct SpcInstrumentRecord(int SourceOrNoiseRate, int Adsr1, int Adsr2, int Gain, int PitchBase)
{
    /// <summary>Sample source number, or the noise rate, 0 through 255.</summary>
    public int SourceOrNoiseRate { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SourceOrNoiseRate);

    /// <summary>VxADSR1 value, 0 through 255.</summary>
    public int Adsr1 { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Adsr1);

    /// <summary>VxADSR2 value, 0 through 255.</summary>
    public int Adsr2 { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Adsr2);

    /// <summary>VxGAIN value, 0 through 255.</summary>
    public int Gain { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Gain);

    /// <summary>Pitch multiplier, 0 through 65535.</summary>
    public int PitchBase { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, PitchBase);
}
