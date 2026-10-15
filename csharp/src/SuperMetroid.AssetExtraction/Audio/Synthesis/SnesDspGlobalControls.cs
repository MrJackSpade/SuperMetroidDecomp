namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

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
        if ((uint)tap > SnesDspRegisterLayout.VoiceIndexMask)
            throw new ArgumentOutOfRangeException(nameof(tap), tap, "The echo FIR has eight taps.");
        dsp.WriteRegister(unchecked((byte)(SnesDspRegisterLayout.FirstFirCoefficient + tap * SnesDspRegisterLayout.VoiceStride)),
            unchecked((byte)coefficient));
    }

    /// <summary>Writes a typed global register at its exact DSP address.</summary>
    private void Write(DspGlobalRegister register, byte value) => dsp.WriteRegister((byte)register, value);
}
