namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The S-DSP's global registers.</summary>
internal readonly struct SnesDspGlobalControls(ManagedSnesDsp dsp)
{
    /// <summary>MVOLL ($0C) / MVOLR ($1C): signed master volume.</summary>
    public void WriteMasterVolume(DspStereoSide side, int volume) => Write(
        side == DspStereoSide.Left ? DspGlobalRegister.MasterVolumeLeft : DspGlobalRegister.MasterVolumeRight, SignedRegisterByte(volume));

    /// <summary>EVOLL ($2C) / EVOLR ($3C): signed echo return volume.</summary>
    public void WriteEchoVolume(DspStereoSide side, int volume) => Write(
        side == DspStereoSide.Left ? DspGlobalRegister.EchoVolumeLeft : DspGlobalRegister.EchoVolumeRight, SignedRegisterByte(volume));

    /// <summary>KON ($4C): starts the selected voices.</summary>
    public void KeyOn(DspVoiceMask voices) => Write(DspGlobalRegister.KeyOn, voices);

    /// <summary>KOFF ($5C): releases the selected voices.</summary>
    public void KeyOff(DspVoiceMask voices) => Write(DspGlobalRegister.KeyOff, voices);

    /// <summary>FLG ($6C): control bits and noise rate.</summary>
    public void WriteFlags(DspFlags flags) => Write(DspGlobalRegister.Flags, flags.Value);

    /// <summary>EFB ($0D): signed echo feedback.</summary>
    public void WriteEchoFeedback(int feedback) => Write(DspGlobalRegister.EchoFeedback, SignedRegisterByte(feedback));

    /// <summary>PMON ($2D): voices whose pitch the previous voice's output modulates.</summary>
    public void WritePitchModulation(DspVoiceMask voices) => Write(DspGlobalRegister.PitchModulation, voices);

    /// <summary>NON ($3D): voices that play noise instead of their sample.</summary>
    public void WriteNoiseEnable(DspVoiceMask voices) => Write(DspGlobalRegister.NoiseEnable, voices);

    /// <summary>EON ($4D): voices fed into the echo buffer.</summary>
    public void WriteEchoEnable(DspVoiceMask voices) => Write(DspGlobalRegister.EchoEnable, voices);

    /// <summary>DIR ($5D): the APU RAM page of the BRR sample directory.</summary>
    public void WriteSourceDirectory(int page) => Write(DspGlobalRegister.SourceDirectory, page);

    /// <summary>ESA ($6D): the APU RAM page where the echo buffer starts.</summary>
    public void WriteEchoBufferPage(int page) => Write(DspGlobalRegister.EchoBufferAddress, page);

    /// <summary>EDL ($7D): the stored echo delay byte (the DSP uses its low four bits).</summary>
    public void WriteEchoDelay(int delay) => Write(DspGlobalRegister.EchoDelay, delay);

    /// <summary>FIR coefficient <paramref name="tap"/> (0-7) at $0F + 16 × tap: a signed filter coefficient.</summary>
    public void WriteFirCoefficient(int tap, int coefficient)
    {
        if ((uint)tap > SnesDspRegisterLayout.VoiceIndexMask)
            throw new ArgumentOutOfRangeException(nameof(tap), tap, "The echo FIR has eight taps.");
        dsp.WriteRegister(unchecked((byte)(SnesDspRegisterLayout.FirstFirCoefficient + tap * SnesDspRegisterLayout.VoiceStride)),
            SignedRegisterByte(coefficient));
    }

    /// <summary>
    /// The register byte holding a signed value from -128 through 127 in two's complement; a value outside that range
    /// is a managed-driver defect, never a silent truncation.
    /// </summary>
    private static byte SignedRegisterByte(int value) => unchecked((byte)checked((sbyte)value));

    /// <summary>Writes a typed global register at its exact DSP address; a value outside a register byte is a managed-driver defect.</summary>
    private void Write(DspGlobalRegister register, int value) =>
        dsp.WriteRegister(checked((byte)SnesDspRegisterLayout.AddressOf(register)), checked((byte)value));

    /// <summary>Writes a voice-set register; the mask's bits are one register byte.</summary>
    private void Write(DspGlobalRegister register, DspVoiceMask voices) => Write(register, (byte)voices.Bits);
}
