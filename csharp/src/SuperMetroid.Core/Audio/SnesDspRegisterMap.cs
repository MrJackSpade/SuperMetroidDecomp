namespace SuperMetroid.Core.Audio;

/// <summary>
/// Hardware register addresses and field masks for the SNES eight-voice S-DSP.
/// These values describe the chip's public register layout; mixer code should name the
/// register it is handling instead of scattering raw addresses through control flow.
/// </summary>
internal static class SnesDspRegisterMap
{
    /// <summary>Number of addressable S-DSP registers.</summary>
    internal const int RegisterFileSize = 0x80;
    /// <summary>Address distance between corresponding registers on adjacent voices.</summary>
    internal const int VoiceStride = 0x10;
    /// <summary>Mask selecting a register's offset within one voice block.</summary>
    internal const int VoiceRegisterMask = VoiceStride - 1;
    /// <summary>Mask selecting one of the eight voice blocks.</summary>
    internal const int VoiceIndexMask = 7;

    /// <summary>Offsets of per-voice DSP registers relative to a voice block.</summary>
    internal static class Voice
    {
        /// <summary>Signed left-channel voice volume register offset.</summary>
        internal const int VolumeLeft = 0;
        /// <summary>Signed right-channel voice volume register offset.</summary>
        internal const int VolumeRight = 1;
        /// <summary>Low byte of the voice pitch register.</summary>
        internal const int PitchLow = 2;
        /// <summary>High byte of the voice pitch register.</summary>
        internal const int PitchHigh = 3;
        /// <summary>Sample-directory source number for the voice.</summary>
        internal const int SourceNumber = 4;
        /// <summary>ADSR enable, attack, and decay fields.</summary>
        internal const int Adsr1 = 5;
        /// <summary>ADSR sustain rate and level fields.</summary>
        internal const int Adsr2 = 6;
        /// <summary>Direct gain value or envelope mode register.</summary>
        internal const int Gain = 7;
        /// <summary>Read-only current envelope value.</summary>
        internal const int EnvelopeOutput = 8;
        /// <summary>Read-only most recently produced voice sample.</summary>
        internal const int SampleOutput = 9;
        /// <summary>Final writable offset in each voice register block.</summary>
        internal const int LastWritable = Gain;
    }

    /// <summary>Absolute register addresses for shared DSP controls.</summary>
    internal static class Global
    {
        /// <summary>Master left output volume register.</summary>
        internal const byte MasterVolumeLeft = 0x0c;
        /// <summary>Master right output volume register.</summary>
        internal const byte MasterVolumeRight = 0x1c;
        /// <summary>Echo left output volume register.</summary>
        internal const byte EchoVolumeLeft = 0x2c;
        /// <summary>Echo right output volume register.</summary>
        internal const byte EchoVolumeRight = 0x3c;
        /// <summary>Voice key-on bitmask register.</summary>
        internal const byte KeyOn = 0x4c;
        /// <summary>Voice key-off bitmask register.</summary>
        internal const byte KeyOff = 0x5c;
        /// <summary>Global reset, mute, and echo-write control register.</summary>
        internal const byte Flags = 0x6c;
        /// <summary>Read-only voice BRR end-flag bitmask.</summary>
        internal const byte EndFlags = 0x7c;
        /// <summary>Echo feedback volume register.</summary>
        internal const byte EchoFeedback = 0x0d;
        /// <summary>Voice pitch-modulation enable bitmask.</summary>
        internal const byte PitchModulation = 0x2d;
        /// <summary>DSP noise generation voice bitmask.</summary>
        internal const byte NoiseEnable = 0x3d;
        /// <summary>Echo routing voice bitmask.</summary>
        internal const byte EchoEnable = 0x4d;
        /// <summary>Base address of the sample directory in APU RAM, in 256-byte pages.</summary>
        internal const byte SourceDirectory = 0x5d;
        /// <summary>Base address of the echo ring buffer in APU RAM, in 256-byte pages.</summary>
        internal const byte EchoBufferAddress = 0x6d;
        /// <summary>Echo ring length expressed in 16-ms units.</summary>
        internal const byte EchoDelay = 0x7d;
        /// <summary>Address of FIR coefficient zero; following taps are at successive global-register rows.</summary>
        internal const byte FirstFirCoefficient = 0x0f;
    }

    /// <summary>Bit masks and shifts used to interpret DSP control-register fields.</summary>
    internal static class Fields
    {
        /// <summary>Six pitch bits stored in a voice's high pitch register.</summary>
        internal const int PitchHighMask = 0x3f;
        /// <summary>Fourteen-bit effective voice pitch range.</summary>
        internal const int PitchMask = 0x3fff;
        /// <summary>ADSR attack-rate field mask.</summary>
        internal const int AdsrAttackMask = 0x0f;
        /// <summary>ADSR decay-rate field mask in ADSR1.</summary>
        internal const int AdsrDecayMask = 0x70;
        /// <summary>Bit shift of the ADSR decay-rate field.</summary>
        internal const int AdsrDecayShift = 4;
        /// <summary>ADSR mode enable bit in ADSR1.</summary>
        internal const int AdsrEnabled = 0x80;
        /// <summary>ADSR sustain-rate field mask in ADSR2.</summary>
        internal const int AdsrSustainRateMask = 0x1f;
        /// <summary>ADSR sustain-level field mask in ADSR2.</summary>
        internal const int AdsrSustainLevelMask = 0xe0;
        /// <summary>Bit shift of the ADSR sustain-level field.</summary>
        internal const int AdsrSustainLevelShift = 5;
        /// <summary>Gain mode selector field mask.</summary>
        internal const int GainModeMask = 0x60;
        /// <summary>Bit shift of the gain mode selector.</summary>
        internal const int GainModeShift = 5;
        /// <summary>Seven-bit direct gain or envelope parameter mask.</summary>
        internal const int GainValueMask = 0x7f;
        /// <summary>FLG soft-reset bit.</summary>
        internal const int Reset = 0x80;
        /// <summary>FLG output-mute bit.</summary>
        internal const int Mute = 0x40;
        /// <summary>FLG echo-buffer write-disable bit.</summary>
        internal const int EchoWriteDisable = 0x20;
        /// <summary>Noise clock rate field mask.</summary>
        internal const int NoiseRateMask = 0x1f;
        /// <summary>Echo delay field mask.</summary>
        internal const int EchoDelayMask = 0x0f;
    }
}
