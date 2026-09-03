using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifyManagedSnesDsp()
    {
        VerifyManagedDspResetProducesSilence();
        VerifyManagedDspDecodesConstructedBrrSample();
        VerifyManagedDspRejectsInvalidBoundaries();
    }

    /// <summary>
    /// A reset S-DSP is muted. This catches accidental output from uninitialised voices,
    /// interpolation history, echo RAM, or the noise generator.
    /// </summary>
    private static void VerifyManagedDspResetProducesSilence()
    {
        byte[] apuRam = new byte[0x10000];
        ManagedSnesDsp dsp = new(apuRam);
        for (int index = 0; index < ManagedSnesDsp.NativeStereoFramesPerVideoFrame; index++)
            dsp.Cycle();

        short[] output = new short[1_600];
        dsp.CopyResampledSamples(output, 800);
        for (int index = 0; index < output.Length; index++)
            AssertEqual(0, output[index], $"reset managed S-DSP output sample {index}");
    }

    /// <summary>
    /// Exercises the real directory lookup, BRR nibble expansion, Gaussian interpolation,
    /// direct-gain envelope, pitch stepping, stereo volumes, and end/loop flag path without
    /// depending on a cartridge room or prerecorded waveform.
    /// </summary>
    private static void VerifyManagedDspDecodesConstructedBrrSample()
    {
        const int directoryAddress = 0x1000;
        const int sampleAddress = 0x2000;
        byte[] apuRam = new byte[0x10000];
        apuRam[directoryAddress] = unchecked((byte)sampleAddress);
        apuRam[directoryAddress + 1] = (byte)(sampleAddress >> 8);
        apuRam[directoryAddress + 2] = unchecked((byte)sampleAddress);
        apuRam[directoryAddress + 3] = (byte)(sampleAddress >> 8);

        // Range $C, filter zero, end+loop. Alternating +7/-8 nibbles create an unmistakable
        // non-silent waveform while the loop flag repeatedly exercises the directory restart.
        apuRam[sampleAddress] = 0xc3;
        for (int index = 1; index <= 8; index++)
            apuRam[sampleAddress + index] = 0x78;

        ManagedSnesDsp dsp = new(apuRam);
        dsp.WriteRegister(0x5d, directoryAddress >> 8);
        dsp.WriteRegister(0x00, 0x7f);
        dsp.WriteRegister(0x01, 0x7f);
        dsp.WriteRegister(0x02, 0xff);
        dsp.WriteRegister(0x03, 0x3f);
        dsp.WriteRegister(0x04, 0);
        dsp.WriteRegister(0x05, 0);
        dsp.WriteRegister(0x07, 0x7f);
        dsp.WriteRegister(0x0c, 0x7f);
        dsp.WriteRegister(0x1c, 0x7f);
        dsp.WriteRegister(0x6c, 0x20);
        dsp.WriteRegister(0x4c, 1);

        for (int index = 0; index < ManagedSnesDsp.NativeStereoFramesPerVideoFrame; index++)
            dsp.Cycle();

        short[] output = new short[1_600];
        dsp.CopyResampledSamples(output, 800);
        AssertTrue(output.Any(sample => sample != 0), "constructed BRR waveform is audible");
        AssertEqual(output[0], output[1], "constructed BRR waveform uses equal stereo volumes");
        AssertTrue((dsp.ReadRegister(0x7c) & 1) != 0, "constructed BRR loop raises ENDX");
    }

    private static void VerifyManagedDspRejectsInvalidBoundaries()
    {
        AssertThrows<ArgumentException>(
            () => _ = new ManagedSnesDsp(new byte[0xffff]),
            "managed DSP rejects non-64-KiB APU RAM");

        ManagedSnesDsp dsp = new(new byte[0x10000]);
        AssertThrows<ArgumentOutOfRangeException>(
            () => dsp.WriteRegister(0x80, 0),
            "managed DSP rejects registers beyond $7F");
        AssertThrows<ArgumentOutOfRangeException>(
            () => dsp.CopyResampledSamples(new short[2], 0),
            "managed DSP rejects an empty host frame");
    }
}
