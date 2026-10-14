namespace SuperMetroid.Core.Audio;

/// <summary>
/// Hardware register addresses and field masks for the SNES eight-voice S-DSP.
/// These values describe the chip's public register layout; mixer code should name the
/// register it is handling instead of scattering raw addresses through control flow.
/// </summary>
[DspRegisterAccess]
internal static class SnesDspRegisterMap
{
    /// <summary>The S-DSP exposes 128 byte-addressed registers.</summary>
    internal const int RegisterFileSize = 0x80;
    /// <summary>Each of eight voices occupies one sixteen-register block.</summary>
    internal const int VoiceStride = 0x10;
    /// <summary>Selects the register offset within a voice block.</summary>
    internal const int VoiceRegisterMask = VoiceStride - 1;
    /// <summary>Constrains a voice number to the chip's eight hardware channels.</summary>
    internal const int VoiceIndexMask = 7;

    /// <summary>Voice register offsets for address arithmetic (voice block + offset).</summary>
    internal static class Voice
    {
        /// <summary>Attack, decay, and ADSR enable bits within each voice block.</summary>
        internal const int Adsr1 = (int)DspVoiceRegister.Adsr1;
        /// <summary>Sustain level and rate bits within each voice block.</summary>
        internal const int Adsr2 = (int)DspVoiceRegister.Adsr2;
        /// <summary>DSP-published current envelope byte.</summary>
        internal const int EnvelopeOutput = (int)DspVoiceRegister.EnvelopeOutput;
        /// <summary>DSP-published current sample byte.</summary>
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

    /// <summary>Bit masks and shifts for pitch, envelopes, gain, DSP flags, and echo delay.</summary>
    internal static class Fields
    {
        /// <summary>Pitch is fourteen bits across its low and high voice registers.</summary>
        internal const int PitchMask = 0x3fff;
        /// <summary>Low ADSR1 nibble selecting the attack rate.</summary>
        internal const int AdsrAttackMask = 0x0f;
        /// <summary>ADSR1 bits selecting the decay rate.</summary>
        internal const int AdsrDecayMask = 0x70;
        /// <summary>Moves the decay-rate bits down to a zero-based value.</summary>
        internal const int AdsrDecayShift = 4;
        /// <summary>ADSR1 high bit selecting ADSR instead of direct gain.</summary>
        internal const int AdsrEnabled = 0x80;
        /// <summary>Low ADSR2 bits selecting the sustain rate.</summary>
        internal const int AdsrSustainRateMask = 0x1f;
        /// <summary>High ADSR2 bits selecting the sustain level.</summary>
        internal const int AdsrSustainLevelMask = 0xe0;
        /// <summary>Moves the sustain-level field to its zero-based value.</summary>
        internal const int AdsrSustainLevelShift = 5;
        /// <summary>GAIN bits selecting the envelope adjustment mode.</summary>
        internal const int GainModeMask = 0x60;
        /// <summary>Moves the GAIN mode field to its zero-based value.</summary>
        internal const int GainModeShift = 5;
        /// <summary>Direct gain value or envelope adjustment rate in the low seven bits.</summary>
        internal const int GainValueMask = 0x7f;
        /// <summary>FLG bit that resets the DSP.</summary>
        internal const int Reset = 0x80;
        /// <summary>FLG bit that mutes audio output.</summary>
        internal const int Mute = 0x40;
        /// <summary>FLG bit that disables echo-buffer writes.</summary>
        internal const int EchoWriteDisable = 0x20;
        /// <summary>FLG low bits selecting the noise generator rate.</summary>
        internal const int NoiseRateMask = 0x1f;
        /// <summary>EDL low nibble selecting echo-buffer delay.</summary>
        internal const int EchoDelayMask = 0x0f;
    }
}
