namespace SuperMetroid.Core.Audio;

/// <summary>The ten registers at the low offsets of each voice's sixteen-byte block.</summary>
[DspRegisterAccess]
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

/// <summary>The S-DSP's named global registers, valued by their register-file address.</summary>
[DspRegisterAccess]
internal enum DspGlobalRegister : byte
{
    /// <summary>MVOLL: left master volume.</summary>
    MasterVolumeLeft = 0x0c,
    /// <summary>MVOLR: right master volume.</summary>
    MasterVolumeRight = 0x1c,
    /// <summary>EVOLL: left echo volume.</summary>
    EchoVolumeLeft = 0x2c,
    /// <summary>EVOLR: right echo volume.</summary>
    EchoVolumeRight = 0x3c,
    /// <summary>KON: key-on voice bits.</summary>
    KeyOn = 0x4c,
    /// <summary>KOFF: key-off voice bits.</summary>
    KeyOff = 0x5c,
    /// <summary>FLG: reset, mute, echo-write and noise-rate flags.</summary>
    Flags = 0x6c,
    /// <summary>ENDX: voice end flags, published by the DSP.</summary>
    EndFlags = 0x7c,
    /// <summary>EFB: echo feedback.</summary>
    EchoFeedback = 0x0d,
    /// <summary>PMON: pitch-modulation voice bits.</summary>
    PitchModulation = 0x2d,
    /// <summary>NON: noise voice bits.</summary>
    NoiseEnable = 0x3d,
    /// <summary>EON: echo voice bits.</summary>
    EchoEnable = 0x4d,
    /// <summary>DIR: BRR sample directory page.</summary>
    SourceDirectory = 0x5d,
    /// <summary>ESA: echo buffer page.</summary>
    EchoBufferAddress = 0x6d,
    /// <summary>EDL: echo delay.</summary>
    EchoDelay = 0x7d,
}

/// <summary>
/// Hardware register addresses and field masks for the SNES eight-voice S-DSP.
/// These values describe the chip's public register layout; mixer code should name the
/// register it is handling instead of scattering raw addresses through control flow.
/// </summary>
[DspRegisterAccess]
internal static class SnesDspRegisterMap
{
    /// <summary>Stores register file size.</summary>
    internal const int RegisterFileSize = 0x80;
    /// <summary>Stores voice stride.</summary>
    internal const int VoiceStride = 0x10;
    /// <summary>Stores voice register mask.</summary>
    internal const int VoiceRegisterMask = VoiceStride - 1;
    /// <summary>Stores voice index mask.</summary>
    internal const int VoiceIndexMask = 7;

    /// <summary>Voice register offsets for address arithmetic (voice block + offset).</summary>
    internal static class Voice
    {
        /// <summary>Stores adsr1.</summary>
        internal const int Adsr1 = (int)DspVoiceRegister.Adsr1;
        /// <summary>Stores adsr2.</summary>
        internal const int Adsr2 = (int)DspVoiceRegister.Adsr2;
        /// <summary>Stores envelope output.</summary>
        internal const int EnvelopeOutput = (int)DspVoiceRegister.EnvelopeOutput;
        /// <summary>Stores sample output.</summary>
        internal const int SampleOutput = (int)DspVoiceRegister.SampleOutput;
    }

    /// <summary>The voice register an address names, or null for offsets $A-$F (global registers).</summary>
    internal static DspVoiceRegister? VoiceRegisterAt(byte address)
    {
        int offset = address & VoiceRegisterMask;
        return offset <= Voice.SampleOutput ? (DspVoiceRegister)offset : null;
    }

    /// <summary>FIR coefficient C0; coefficient i is at this offset in voice block i.</summary>
    internal const byte FirstFirCoefficient = 0x0f;

    /// <summary>The named global register an address selects, or null for FIR coefficients and unused addresses.</summary>
    internal static DspGlobalRegister? GlobalRegisterAt(byte address) =>
        Enum.IsDefined((DspGlobalRegister)address) ? (DspGlobalRegister)address : null;

    /// <summary>Represents fields.</summary>
    internal static class Fields
    {
        /// <summary>Stores pitch mask.</summary>
        internal const int PitchMask = 0x3fff;
        /// <summary>Stores adsr attack mask.</summary>
        internal const int AdsrAttackMask = 0x0f;
        /// <summary>Stores adsr decay mask.</summary>
        internal const int AdsrDecayMask = 0x70;
        /// <summary>Stores adsr decay shift.</summary>
        internal const int AdsrDecayShift = 4;
        /// <summary>Stores adsr enabled.</summary>
        internal const int AdsrEnabled = 0x80;
        /// <summary>Stores adsr sustain rate mask.</summary>
        internal const int AdsrSustainRateMask = 0x1f;
        /// <summary>Stores adsr sustain level mask.</summary>
        internal const int AdsrSustainLevelMask = 0xe0;
        /// <summary>Stores adsr sustain level shift.</summary>
        internal const int AdsrSustainLevelShift = 5;
        /// <summary>Stores gain mode mask.</summary>
        internal const int GainModeMask = 0x60;
        /// <summary>Stores gain mode shift.</summary>
        internal const int GainModeShift = 5;
        /// <summary>Stores gain value mask.</summary>
        internal const int GainValueMask = 0x7f;
        /// <summary>Stores reset.</summary>
        internal const int Reset = 0x80;
        /// <summary>Stores mute.</summary>
        internal const int Mute = 0x40;
        /// <summary>Stores echo write disable.</summary>
        internal const int EchoWriteDisable = 0x20;
        /// <summary>Stores noise rate mask.</summary>
        internal const int NoiseRateMask = 0x1f;
        /// <summary>Stores echo delay mask.</summary>
        internal const int EchoDelayMask = 0x0f;
    }
}
