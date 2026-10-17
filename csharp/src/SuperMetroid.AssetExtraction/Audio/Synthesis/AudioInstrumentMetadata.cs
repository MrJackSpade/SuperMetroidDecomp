using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The six bytes consumed by the SPC driver's set-instrument command.</summary>
public sealed record AudioInstrumentMetadata(
    int Instrument,
    bool UsesNoise,
    int SourceOrNoiseRate,
    int Adsr1,
    int Adsr2,
    int Gain,
    int PitchBase)
{
    /// <summary>The sample source number, or the noise rate when the instrument uses noise, 0 through 255.</summary>
    public int SourceOrNoiseRate { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SourceOrNoiseRate);

    /// <summary>The VxADSR1 byte, 0 through 255.</summary>
    public int Adsr1 { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Adsr1);

    /// <summary>The VxADSR2 byte, 0 through 255.</summary>
    public int Adsr2 { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Adsr2);

    /// <summary>The VxGAIN byte, 0 through 255.</summary>
    public int Gain { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Gain);

    /// <summary>The pitch multiplier, 0 through 65535.</summary>
    public int PitchBase { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, PitchBase);
}
