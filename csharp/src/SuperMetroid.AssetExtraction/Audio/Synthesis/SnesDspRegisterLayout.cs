using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Hardware register addresses and field masks for the SNES eight-voice S-DSP.
/// These values describe the chip's public register layout; mixer code should name the
/// register it is handling instead of scattering raw addresses through control flow.
/// </summary>
[EncodingOwner("S-DSP register file addresses and field masks")]
internal static class SnesDspRegisterLayout
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
    internal static DspVoiceRegister? VoiceRegisterAt(int address)
    {
        int offset = address & VoiceRegisterMask;
        return offset <= Voice.SampleOutput ? (DspVoiceRegister)offset : null;
    }

    /// <summary>FIR coefficient C0; coefficient i is at this offset in voice block i.</summary>
    internal const byte FirstFirCoefficient = 0x0f;

    /// <summary>Each named global register's address in the register file.</summary>
    private static readonly Dictionary<DspGlobalRegister, int> globalRegisterAddresses = new()
    {
        [DspGlobalRegister.MasterVolumeLeft] = 0x0c,
        [DspGlobalRegister.MasterVolumeRight] = 0x1c,
        [DspGlobalRegister.EchoVolumeLeft] = 0x2c,
        [DspGlobalRegister.EchoVolumeRight] = 0x3c,
        [DspGlobalRegister.KeyOn] = 0x4c,
        [DspGlobalRegister.KeyOff] = 0x5c,
        [DspGlobalRegister.Flags] = 0x6c,
        [DspGlobalRegister.EndFlags] = 0x7c,
        [DspGlobalRegister.EchoFeedback] = 0x0d,
        [DspGlobalRegister.PitchModulation] = 0x2d,
        [DspGlobalRegister.NoiseEnable] = 0x3d,
        [DspGlobalRegister.EchoEnable] = 0x4d,
        [DspGlobalRegister.SourceDirectory] = 0x5d,
        [DspGlobalRegister.EchoBufferAddress] = 0x6d,
        [DspGlobalRegister.EchoDelay] = 0x7d,
    };

    /// <summary>Each address that selects a named global register, by address.</summary>
    private static readonly Dictionary<int, DspGlobalRegister> globalRegistersByAddress =
        globalRegisterAddresses.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>The register-file address of <paramref name="register"/>.</summary>
    internal static int AddressOf(DspGlobalRegister register) => globalRegisterAddresses[register];

    /// <summary>The named global register an address selects, or null for FIR coefficients and unused addresses.</summary>
    internal static DspGlobalRegister? GlobalRegisterAt(int address) =>
        globalRegistersByAddress.TryGetValue(address, out DspGlobalRegister register) ? register : null;

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

    /// <summary>$7FF: the largest eleven-bit envelope level.</summary>
    internal const int EnvelopeMaximum = 0x7ff;

    /// <summary>$7E0: the attack level at which the envelope begins its decay.</summary>
    internal const int AttackEndLevel = 0x7e0;

    /// <summary>$600: the bent-line gain level above which the increase slows from 32 to 8.</summary>
    internal const int BentLineThreshold = 0x600;

    /// <summary>$100: the envelope level per sustain-level step.</summary>
    internal const int SustainLevelStep = 0x100;

    /// <summary>$400: the neutral pitch-modulation factor, added to the previous voice's output.</summary>
    internal const int PitchModulationBias = 0x400;

    /// <summary>-$4000: the noise generator's reset sample.</summary>
    internal const int NoiseResetSample = -0x4000;

    /// <summary>$3FFF: the noise shift register bits kept as it shifts right.</summary>
    internal const int NoiseShiftMask = 0x3fff;

    /// <summary>$7FFF: the noise register's fifteen sample bits.</summary>
    internal const int NoiseSampleMask = 0x7fff;
}
