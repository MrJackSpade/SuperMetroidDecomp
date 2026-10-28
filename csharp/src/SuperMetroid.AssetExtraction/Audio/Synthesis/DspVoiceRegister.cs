namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The ten registers at the low offsets of each voice's sixteen-byte block.</summary>
internal enum DspVoiceRegister
{
    /// <summary>VxVOLL: left volume.</summary>
    VolumeLeft = 0,
    /// <summary>VxVOLR: right volume.</summary>
    VolumeRight = 1,
    /// <summary>VxPITCHL: pitch low byte.</summary>
    PitchLow = 2,
    /// <summary>VxPITCHH: pitch high bits.</summary>
    PitchHigh = 3,
    /// <summary>VxSRCN: sample source number.</summary>
    SourceNumber = 4,
    /// <summary>VxADSR1.</summary>
    Adsr1 = 5,
    /// <summary>VxADSR2.</summary>
    Adsr2 = 6,
    /// <summary>VxGAIN.</summary>
    Gain = 7,
    /// <summary>VxENVX: current envelope, published by the DSP.</summary>
    EnvelopeOutput = 8,
    /// <summary>VxOUTX: current sample, published by the DSP.</summary>
    SampleOutput = 9,
}
