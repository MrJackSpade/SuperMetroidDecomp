using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks typed S-DSP controls against their exact register-file writes.</summary>
internal static class DspControlsVerification
{
    /// <summary>
    /// #631: every typed S-DSP control writes exactly its register, for all eight voices and eight FIR
    /// taps; pitch and ADSR keep their native write order; FLG's control bits and noise rate are two
    /// views of one register; out-of-range voices and taps are rejected; and typed writes allocate nothing.
    /// </summary>
    internal static void VerifyDspControls()
    {
        var dsp = new ManagedSnesDsp(new byte[0x10000]);
        var controls = new SnesDspControls(dsp);
        SnesDspGlobalControls global = controls.Global;

        void Expect(int address, byte value, string what) =>
            AssertEqual(value, dsp.ReadRegister(unchecked((byte)address)), $"{what} writes ${address:X2}");

        global.WriteMasterVolume(DspStereoSide.Left, -3);
        Expect(0x0c, 0xfd, "master volume left");
        global.WriteMasterVolume(DspStereoSide.Right, 0x45);
        Expect(0x1c, 0x45, "master volume right");
        global.WriteEchoVolume(DspStereoSide.Left, -128);
        Expect(0x2c, 0x80, "echo volume left");
        global.WriteEchoVolume(DspStereoSide.Right, 0x12);
        Expect(0x3c, 0x12, "echo volume right");
        global.KeyOff(new DspVoiceMask(0xa5));
        Expect(0x5c, 0xa5, "key-off");
        global.WriteEchoFeedback(-77);
        Expect(0x0d, unchecked((byte)-77), "echo feedback");
        global.WritePitchModulation(new DspVoiceMask(0x06));
        Expect(0x2d, 0x06, "pitch modulation");
        global.WriteNoiseEnable(new DspVoiceMask(0x81));
        Expect(0x3d, 0x81, "noise enable");
        global.WriteEchoEnable(new DspVoiceMask(0x7e));
        Expect(0x4d, 0x7e, "echo enable");
        global.WriteSourceDirectory(0x6d);
        Expect(0x5d, 0x6d, "source directory");
        global.WriteEchoBufferPage(0xd0);
        Expect(0x6d, 0xd0, "echo buffer page");
        global.WriteEchoDelay(0x24);
        Expect(0x7d, 0x24, "echo delay");
        AssertEqual(4, SnesDspControls.EchoDelayOf(0x24), "EDL uses its low four bits");
        for (int tap = 0; tap < 8; tap++)
        {
            global.WriteFirCoefficient(tap, unchecked((sbyte)(0x10 + tap)));
            Expect(0x0f + 16 * tap, unchecked((byte)(0x10 + tap)), $"FIR tap {tap}");
        }

        // FLG: the control bits and noise rate are separately named views of one register byte.
        var flags = new DspFlags(0xe5);
        global.WriteFlags(flags);
        Expect(0x6c, 0xe5, "flags");
        AssertEqual((byte)0xe0, flags.Control, "FLG control bits alias the register's high three bits");
        AssertEqual(5, flags.NoiseRate, "FLG noise rate aliases the register's low five bits");
        AssertEqual(0x1f, DspFlags.NoiseRateOf(0xff), "an instrument's noise rate is its low five bits");

        for (int voice = 0; voice < 8; voice++)
        {
            SnesDspVoiceControls controlsForVoice = controls.Voice(voice);
            int block = voice * 16;
            controlsForVoice.WriteVolume(DspStereoSide.Left, -1);
            Expect(block + 0, 0xff, $"voice {voice} volume left");
            controlsForVoice.WriteVolume(DspStereoSide.Right, 0x33);
            Expect(block + 1, 0x33, $"voice {voice} volume right");
            controlsForVoice.WritePitch(0x1234);
            Expect(block + 2, 0x34, $"voice {voice} pitch low");
            Expect(block + 3, 0x12, $"voice {voice} pitch high");
            controlsForVoice.WriteSource(unchecked((byte)(0x20 + voice)));
            Expect(block + 4, unchecked((byte)(0x20 + voice)), $"voice {voice} source");
            controlsForVoice.WriteAdsr(0x8f, 0xe0);
            Expect(block + 5, 0x8f, $"voice {voice} ADSR1");
            Expect(block + 6, 0xe0, $"voice {voice} ADSR2");
            controlsForVoice.WriteGain(0x7f);
            Expect(block + 7, 0x7f, $"voice {voice} gain");
        }

        AssertThrows<ArgumentOutOfRangeException>(() => controls.Voice(8), "the S-DSP has eight voices");
        AssertThrows<ArgumentOutOfRangeException>(() => controls.Voice(-1), "voice indices are non-negative");
        AssertThrows<ArgumentOutOfRangeException>(() => global.WriteFirCoefficient(8, 0), "the echo FIR has eight taps");

        // The typed controls are structs over the DSP; a burst of writes allocates nothing.
        for (int warm = 0; warm < 4; warm++)
            WriteTypedDspBurst(controls);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int burst = 0; burst < 1_000; burst++)
            WriteTypedDspBurst(controls);
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before, "typed DSP writes allocate nothing");
    }

    /// <summary>Issues a representative eight-voice write burst for the allocation check.</summary>
    private static void WriteTypedDspBurst(SnesDspControls controls)
    {
        for (int voice = 0; voice < 8; voice++)
        {
            SnesDspVoiceControls voiceControls = controls.Voice(voice);
            voiceControls.WriteVolume(DspStereoSide.Left, 0x10);
            voiceControls.WritePitch(0x1000);
            voiceControls.WriteGain(0x40);
        }
        controls.Global.WriteEchoFeedback(0);
    }

    /// <summary>Runs the typed S-DSP controls verification.</summary>
    [VerificationCommand("--dsp-controls")]
    internal static int DspControlsCommand()
    {
        Suite(nameof(VerifyDspControls), () => VerifyDspControls());
        return 0;
    }
}
