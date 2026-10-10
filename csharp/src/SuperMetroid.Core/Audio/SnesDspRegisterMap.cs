namespace SuperMetroid.Core.Audio;

/// <summary>The ten registers at the low offsets of each voice's sixteen-byte block.</summary>
internal enum DspVoiceRegister : byte
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

/// <summary>
/// Hardware register addresses and field masks for the SNES eight-voice S-DSP.
/// These values describe the chip's public register layout; mixer code should name the
/// register it is handling instead of scattering raw addresses through control flow.
/// </summary>
internal static class SnesDspRegisterMap
{
    internal const int RegisterFileSize = 0x80;
    internal const int VoiceStride = 0x10;
    internal const int VoiceRegisterMask = VoiceStride - 1;
    internal const int VoiceIndexMask = 7;

    /// <summary>Voice register offsets for address arithmetic (voice block + offset).</summary>
    internal static class Voice
    {
        internal const int VolumeLeft = (int)DspVoiceRegister.VolumeLeft;
        internal const int PitchLow = (int)DspVoiceRegister.PitchLow;
        internal const int PitchHigh = (int)DspVoiceRegister.PitchHigh;
        internal const int SourceNumber = (int)DspVoiceRegister.SourceNumber;
        internal const int Adsr1 = (int)DspVoiceRegister.Adsr1;
        internal const int Adsr2 = (int)DspVoiceRegister.Adsr2;
        internal const int Gain = (int)DspVoiceRegister.Gain;
        internal const int EnvelopeOutput = (int)DspVoiceRegister.EnvelopeOutput;
        internal const int SampleOutput = (int)DspVoiceRegister.SampleOutput;
    }

    /// <summary>The voice register an address names, or null for offsets $A-$F (global registers).</summary>
    internal static DspVoiceRegister? VoiceRegisterAt(byte address)
    {
        int offset = address & VoiceRegisterMask;
        return offset <= Voice.SampleOutput ? (DspVoiceRegister)offset : null;
    }

    internal static class Global
    {
        internal const byte MasterVolumeLeft = 0x0c;
        internal const byte MasterVolumeRight = 0x1c;
        internal const byte EchoVolumeLeft = 0x2c;
        internal const byte EchoVolumeRight = 0x3c;
        internal const byte KeyOn = 0x4c;
        internal const byte KeyOff = 0x5c;
        internal const byte Flags = 0x6c;
        internal const byte EndFlags = 0x7c;
        internal const byte EchoFeedback = 0x0d;
        internal const byte PitchModulation = 0x2d;
        internal const byte NoiseEnable = 0x3d;
        internal const byte EchoEnable = 0x4d;
        internal const byte SourceDirectory = 0x5d;
        internal const byte EchoBufferAddress = 0x6d;
        internal const byte EchoDelay = 0x7d;
        internal const byte FirstFirCoefficient = 0x0f;
    }

    internal static class Fields
    {
        internal const int PitchHighMask = 0x3f;
        internal const int PitchMask = 0x3fff;
        internal const int AdsrAttackMask = 0x0f;
        internal const int AdsrDecayMask = 0x70;
        internal const int AdsrDecayShift = 4;
        internal const int AdsrEnabled = 0x80;
        internal const int AdsrSustainRateMask = 0x1f;
        internal const int AdsrSustainLevelMask = 0xe0;
        internal const int AdsrSustainLevelShift = 5;
        internal const int GainModeMask = 0x60;
        internal const int GainModeShift = 5;
        internal const int GainValueMask = 0x7f;
        internal const int Reset = 0x80;
        internal const int Mute = 0x40;
        internal const int EchoWriteDisable = 0x20;
        internal const int NoiseRateMask = 0x1f;
        internal const int EchoDelayMask = 0x0f;
    }
}
