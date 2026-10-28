using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One S-DSP voice's registers, in its sixteen-byte block at $00 + 16 × voice.</summary>
internal readonly struct SnesDspVoiceControls
{
    /// <summary>DSP register file receiving writes for the selected voice.</summary>
    private readonly ManagedSnesDsp dsp;
    /// <summary>Sixteen-byte base address of this voice's register block.</summary>
    private readonly int block;

    /// <summary>Binds voice <paramref name="index"/> (0-7).</summary>
    public SnesDspVoiceControls(ManagedSnesDsp dsp, int index)
    {
        this.dsp = Ensure.NotNull(dsp);
        if ((uint)index > SnesDspRegisterLayout.VoiceIndexMask)
            throw new ArgumentOutOfRangeException(nameof(index), index, "The S-DSP has eight voices.");
        block = index * SnesDspRegisterLayout.VoiceStride;
    }

    /// <summary>VxVOLL ($x0) / VxVOLR ($x1): signed voice volume.</summary>
    public void WriteVolume(DspStereoSide side, sbyte volume) =>
        Write(side == DspStereoSide.Left ? DspVoiceRegister.VolumeLeft : DspVoiceRegister.VolumeRight, unchecked((byte)volume));

    /// <summary>VxPITCHL ($x2) then VxPITCHH ($x3): the pitch word, low byte first as the driver writes it.</summary>
    public void WritePitch(ushort pitch)
    {
        Write(DspVoiceRegister.PitchLow, unchecked((byte)pitch));
        Write(DspVoiceRegister.PitchHigh, SnesLittleEndian.HighByte(pitch));
    }

    /// <summary>VxSRCN ($x4): the sample directory entry the voice plays.</summary>
    public void WriteSource(int source) => Write(DspVoiceRegister.SourceNumber, checked((byte)source));

    /// <summary>VxADSR1 ($x5) then VxADSR2 ($x6): the envelope configuration, first byte first.</summary>
    public void WriteAdsr(int adsr1, int adsr2)
    {
        Write(DspVoiceRegister.Adsr1, checked((byte)adsr1));
        Write(DspVoiceRegister.Adsr2, checked((byte)adsr2));
    }

    /// <summary>VxGAIN ($x7): the gain mode and value used when ADSR is disabled.</summary>
    public void WriteGain(int gain) => Write(DspVoiceRegister.Gain, checked((byte)gain));

    /// <summary>Offsets a typed voice register from this voice's sixteen-byte block.</summary>
    private void Write(DspVoiceRegister register, byte value) => dsp.WriteRegister((byte)(block + (int)register), value);
}
