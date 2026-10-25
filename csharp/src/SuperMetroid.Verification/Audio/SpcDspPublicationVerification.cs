using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.Verification.Audio;

/// <summary>Checks SPC player DSP register publication and delayed echo writes against native mapping.</summary>
internal static class SpcDspPublicationVerification
{
    /// <summary>Checks unconditional and gated register writes, plus clearing of pending key-on/off bytes.</summary>
    internal static void VerifySpcDspPublication(ISnesAddressSpace rom)
    {
        foreach (byte stored in new byte[] { 0x80, 1, 2 })
        {
            var player = new ManagedSpcPlayer();
            var dsp = player.dsp;
            dsp.SetSampleBank(new ManagedPcmSampleBank("publication-fixture", 0,
                new Dictionary<int, ManagedPcmSample>
                {
                    [0] = new ManagedPcmSample("silent", 32000, new short[16], 0),
                }));
            var direct = new byte[256];
            void Set(ref int register, int address, byte value)
            {
                direct[address] = value;
                register = value;
            }
            Set(ref player.Voices.keyOff, 0x46, 0x81);
            Set(ref player.Voices.keyOn, 0x45, 0x42);
            Set(ref player.Voices.noiseEnable, 0x49, 0x24);
            Set(ref player.Voices.dspFlags, 0x48, 0x17);
            Set(ref player.Voices.echoEnable, 0x4a, 0x18);
            Set(ref player.Music.echoFeedback, 0x4e, 0xb3);
            player.Music.echoVolumeLeft = 0x3567;
            player.Music.echoVolumeRight = 0xa912;
            direct[0x61] = 0x35;
            direct[0x63] = 0xa9;
            player.Music.echoStoredTime = stored;
            player.Music.echoDelay = 2;
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
            AssertEqual(0, player.Voices.keyOff, "pending key-off clears after publication");
            AssertEqual(0, player.Voices.keyOn, "pending key-on clears after publication");

        }
    }
}
