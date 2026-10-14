using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

/// <summary>Checks SPC player DSP register publication and delayed echo writes against native mapping.</summary>
internal static class SpcDspPublicationVerification
{
    /// <summary>Checks unconditional and gated register writes, plus clearing of pending key-on/off bytes.</summary>
    [DspRegisterBoundary("exact register test: checks S-DSP register behaviour")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for SPC DSP publication")]
    internal static void VerifySpcDspPublication(ISnesAddressSpace rom)
    {
        foreach (byte stored in new byte[] { 0x80, 1, 2 })
        {
            var player = new ManagedSpcPlayer();
            var dsp = player.dsp;
            dsp.SetSampleBank(new ManagedPcmSampleBank("publication-fixture", 0,
                new Dictionary<byte, ManagedPcmSample>
                {
                    [0] = new ManagedPcmSample("silent", 32000, new short[16], 0),
                }));
            var direct = new byte[256];
            void Set(ref byte register, int address, byte value)
            {
                direct[address] = value;
                register = value;
            }
            Set(ref player.keyOff, 0x46, 0x81);
            Set(ref player.keyOn, 0x45, 0x42);
            Set(ref player.noiseEnable, 0x49, 0x24);
            Set(ref player.dspFlags, 0x48, 0x17);
            Set(ref player.echoEnable, 0x4a, 0x18);
            Set(ref player.echoFeedback, 0x4e, 0xb3);
            player.echoVolumeLeft = 0x3567;
            player.echoVolumeRight = 0xa912;
            direct[0x61] = 0x35;
            direct[0x63] = 0xa9;
            player.echoStoredTime = stored;
            player.echoDelay = 2;
            // Seed all destinations so skipped echo writes are observable.
            for (int index = 0; index < 10; index++)
                dsp.WriteRegister(rom.ReadCartridgeByte(0xcf8a5a + index), 0x55);
            byte[] expected = new byte[128];
            for (int address = 0; address < 128; address++) expected[address] = dsp.ReadRegister((byte)address);
            // Native descending map: the first five writes are unconditional,
            // then FLG, then the four echo values once the delay has settled.
            int last = (stored & 0x80) != 0 ? 5 : stored == 2 ? 0 : 4;
            for (int index = 9; index >= last; index--)
                expected[rom.ReadCartridgeByte(0xcf8a5a + index)] = direct[rom.ReadCartridgeByte(0xcf8a64 + index)];
            player.LoopPartOne();
            for (int address = 0; address < 128; address++)
                AssertEqual(expected[address], dsp.ReadRegister((byte)address), "native DSP publication map and echo gating");
            AssertEqual((byte)0, player.keyOff, "pending key-off clears after publication");
            AssertEqual((byte)0, player.keyOn, "pending key-on clears after publication");

        }
    }
}
