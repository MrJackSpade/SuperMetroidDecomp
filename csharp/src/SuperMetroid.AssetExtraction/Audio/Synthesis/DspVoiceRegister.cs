namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The ten registers at the low offsets of each voice's sixteen-byte block.</summary>
internal enum DspVoiceRegister
{
    /// <summary>VxVOLL: left volume.</summary>
    VolumeLeft,
    /// <summary>VxVOLR: right volume.</summary>
    VolumeRight,
    /// <summary>VxPITCHL: pitch low byte.</summary>
    PitchLow,
    /// <summary>VxPITCHH: pitch high bits.</summary>
    PitchHigh,
    /// <summary>VxSRCN: sample source number.</summary>
    SourceNumber,
    /// <summary>VxADSR1.</summary>
    Adsr1,
    /// <summary>VxADSR2.</summary>
    Adsr2,
    /// <summary>VxGAIN.</summary>
    Gain,
    /// <summary>VxENVX: current envelope, published by the DSP.</summary>
    EnvelopeOutput,
    /// <summary>VxOUTX: current sample, published by the DSP.</summary>
    SampleOutput,
}
