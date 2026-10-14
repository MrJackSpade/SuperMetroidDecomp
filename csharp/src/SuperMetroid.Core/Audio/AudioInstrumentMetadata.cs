namespace SuperMetroid.Core.Audio;

/// <summary>The six bytes consumed by the SPC driver's set-instrument command.</summary>
public sealed record AudioInstrumentMetadata(
    int Instrument,
    bool UsesNoise,
    byte SourceOrNoiseRate,
    byte Adsr1,
    byte Adsr2,
    byte Gain,
    ushort PitchBase);
