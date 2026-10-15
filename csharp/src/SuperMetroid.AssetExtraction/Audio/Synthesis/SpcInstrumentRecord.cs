namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One six-byte instrument record of the driver's instrument table (#632).</summary>
/// <param name="SourceOrNoiseRate">Sample source number, or the noise rate when <see cref="SpcDriverDefinitions.Instruments.NoiseMarker"/> is set.</param>
/// <param name="Adsr1">VxADSR1 value.</param>
/// <param name="Adsr2">VxADSR2 value.</param>
/// <param name="Gain">VxGAIN value.</param>
/// <param name="PitchBase">Pitch multiplier, stored big-endian in the record.</param>
internal readonly record struct SpcInstrumentRecord(byte SourceOrNoiseRate, byte Adsr1, byte Adsr2, byte Gain, ushort PitchBase);
