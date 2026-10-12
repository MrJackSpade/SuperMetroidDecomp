namespace SuperMetroid.Core.Audio;

/// <summary>Which output channel a stereo volume register drives.</summary>
internal enum DspStereoSide
{
    /// <summary>The left output (VxVOLL, MVOLL, EVOLL).</summary>
    Left = 0,

    /// <summary>The right output (VxVOLR, MVOLR, EVOLR).</summary>
    Right = 1,
}

/// <summary>A set of the eight voices, one bit per voice: the KON, KOFF, PMON, NON, EON and ENDX layout.</summary>
/// <param name="Bits">Bit <c>n</c> selects voice <c>n</c>.</param>
internal readonly record struct DspVoiceMask(byte Bits)
{
    /// <summary>No voices.</summary>
    public static DspVoiceMask None => default;

    /// <summary>All eight voices.</summary>
    public static DspVoiceMask All => new(byte.MaxValue);
}

/// <summary>
/// FLG ($6C): the reset, mute and echo-write-disable control bits sharing one register with the
/// five-bit noise clock rate. The control bits and the noise rate are separate meanings
/// of the same register byte: the control bits are bits 5-7 and the noise rate bits 0-4.
/// </summary>
/// <param name="Value">The complete FLG byte.</param>
[DspRegisterBoundary("typed controls: FLG control bits and noise rate share one register")]
internal readonly record struct DspFlags(byte Value)
{
    /// <summary>The noise rate an instrument's flag byte selects (its low five bits).</summary>
    public static int NoiseRateOf(byte instrumentFlags) => instrumentFlags & SnesDspRegisterMap.Fields.NoiseRateMask;
}

/// <summary>The S-DSP's global and per-voice registers as typed controls (#631).</summary>
[DspRegisterBoundary("typed controls: S-DSP register file")]
internal readonly struct SnesDspControls(ManagedSnesDsp dsp)
{
    /// <summary>The global (non-voice) registers.</summary>
    public SnesDspGlobalControls Global => new(dsp);

    /// <summary>Voice <paramref name="index"/> (0-7).</summary>
    public SnesDspVoiceControls Voice(int index) => new(dsp, index);

    /// <summary>The EDL ($7D) echo delay steps a stored EDL byte selects (its low four bits).</summary>
    public static int EchoDelayOf(byte echoDelay) => echoDelay & SnesDspRegisterMap.Fields.EchoDelayMask;
}

/// <summary>The S-DSP's global registers.</summary>
[DspRegisterBoundary("typed controls: S-DSP global registers")]
internal readonly struct SnesDspGlobalControls(ManagedSnesDsp dsp)
{
    /// <summary>MVOLL ($0C) / MVOLR ($1C): signed master volume.</summary>
    public void WriteMasterVolume(DspStereoSide side, sbyte volume) => Write(
        side == DspStereoSide.Left ? DspGlobalRegister.MasterVolumeLeft : DspGlobalRegister.MasterVolumeRight, unchecked((byte)volume));

    /// <summary>EVOLL ($2C) / EVOLR ($3C): signed echo return volume.</summary>
    public void WriteEchoVolume(DspStereoSide side, sbyte volume) => Write(
        side == DspStereoSide.Left ? DspGlobalRegister.EchoVolumeLeft : DspGlobalRegister.EchoVolumeRight, unchecked((byte)volume));

    /// <summary>KON ($4C): starts the selected voices.</summary>
    public void KeyOn(DspVoiceMask voices) => Write(DspGlobalRegister.KeyOn, voices.Bits);

    /// <summary>KOFF ($5C): releases the selected voices.</summary>
    public void KeyOff(DspVoiceMask voices) => Write(DspGlobalRegister.KeyOff, voices.Bits);

    /// <summary>FLG ($6C): control bits and noise rate.</summary>
    public void WriteFlags(DspFlags flags) => Write(DspGlobalRegister.Flags, flags.Value);

    /// <summary>EFB ($0D): signed echo feedback.</summary>
    public void WriteEchoFeedback(sbyte feedback) => Write(DspGlobalRegister.EchoFeedback, unchecked((byte)feedback));

    /// <summary>PMON ($2D): voices whose pitch the previous voice's output modulates.</summary>
    public void WritePitchModulation(DspVoiceMask voices) => Write(DspGlobalRegister.PitchModulation, voices.Bits);

    /// <summary>NON ($3D): voices that play noise instead of their sample.</summary>
    public void WriteNoiseEnable(DspVoiceMask voices) => Write(DspGlobalRegister.NoiseEnable, voices.Bits);

    /// <summary>EON ($4D): voices fed into the echo buffer.</summary>
    public void WriteEchoEnable(DspVoiceMask voices) => Write(DspGlobalRegister.EchoEnable, voices.Bits);

    /// <summary>DIR ($5D): the APU RAM page of the BRR sample directory.</summary>
    public void WriteSourceDirectory(byte page) => Write(DspGlobalRegister.SourceDirectory, page);

    /// <summary>ESA ($6D): the APU RAM page where the echo buffer starts.</summary>
    public void WriteEchoBufferPage(byte page) => Write(DspGlobalRegister.EchoBufferAddress, page);

    /// <summary>EDL ($7D): the stored echo delay byte (the DSP uses its low four bits).</summary>
    public void WriteEchoDelay(byte delay) => Write(DspGlobalRegister.EchoDelay, delay);

    /// <summary>FIR coefficient <paramref name="tap"/> (0-7) at $0F + 16 × tap: a signed filter coefficient.</summary>
    public void WriteFirCoefficient(int tap, sbyte coefficient)
    {
        if ((uint)tap > SnesDspRegisterMap.VoiceIndexMask)
            throw new ArgumentOutOfRangeException(nameof(tap), tap, "The echo FIR has eight taps.");
        dsp.WriteRegister(unchecked((byte)(SnesDspRegisterMap.FirstFirCoefficient + tap * SnesDspRegisterMap.VoiceStride)),
            unchecked((byte)coefficient));
    }

    private void Write(DspGlobalRegister register, byte value) => dsp.WriteRegister((byte)register, value);
}

/// <summary>One S-DSP voice's registers, in its sixteen-byte block at $00 + 16 × voice.</summary>
[DspRegisterBoundary("typed controls: one S-DSP voice block")]
internal readonly struct SnesDspVoiceControls
{
    private readonly ManagedSnesDsp dsp;
    private readonly byte block;

    /// <summary>Binds voice <paramref name="index"/> (0-7).</summary>
    public SnesDspVoiceControls(ManagedSnesDsp dsp, int index)
    {
        this.dsp = Ensure.NotNull(dsp);
        if ((uint)index > SnesDspRegisterMap.VoiceIndexMask)
            throw new ArgumentOutOfRangeException(nameof(index), index, "The S-DSP has eight voices.");
        block = unchecked((byte)(index * SnesDspRegisterMap.VoiceStride));
    }

    /// <summary>VxVOLL ($x0) / VxVOLR ($x1): signed voice volume.</summary>
    public void WriteVolume(DspStereoSide side, sbyte volume) =>
        Write(side == DspStereoSide.Left ? DspVoiceRegister.VolumeLeft : DspVoiceRegister.VolumeRight, unchecked((byte)volume));

    /// <summary>VxPITCHL ($x2) then VxPITCHH ($x3): the pitch word, low byte first as the driver writes it.</summary>
    public void WritePitch(ushort pitch)
    {
        Write(DspVoiceRegister.PitchLow, unchecked((byte)pitch));
        Write(DspVoiceRegister.PitchHigh, unchecked((byte)(pitch >> 8)));
    }

    /// <summary>VxSRCN ($x4): the sample directory entry the voice plays.</summary>
    public void WriteSource(byte source) => Write(DspVoiceRegister.SourceNumber, source);

    /// <summary>VxADSR1 ($x5) then VxADSR2 ($x6): the envelope configuration, first byte first.</summary>
    public void WriteAdsr(byte adsr1, byte adsr2)
    {
        Write(DspVoiceRegister.Adsr1, adsr1);
        Write(DspVoiceRegister.Adsr2, adsr2);
    }

    /// <summary>VxGAIN ($x7): the gain mode and value used when ADSR is disabled.</summary>
    public void WriteGain(byte gain) => Write(DspVoiceRegister.Gain, gain);

    private void Write(DspVoiceRegister register, byte value) => dsp.WriteRegister(unchecked((byte)(block + (int)register)), value);
}
