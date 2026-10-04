using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySpcDspPublication(ISnesAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (byte stored in new byte[] { 0x80, 1, 2 })
        {
            var player = new ManagedSpcPlayer();
            var dsp = (ManagedSnesDsp)Field("dsp").GetValue(player)!;
            dsp.SetSampleBank(new ManagedPcmSampleBank("publication-fixture", 0,
                new Dictionary<byte, ManagedPcmSample>
                {
                    [0] = new ManagedPcmSample("silent", 32000, new short[16], 0),
                }));
            var direct = new byte[256];
            Set("keyOff", 0x46, 0x81);
            Set("keyOn", 0x45, 0x42);
            Set("noiseEnable", 0x49, 0x24);
            Set("dspFlags", 0x48, 0x17);
            Set("echoEnable", 0x4a, 0x18);
            Set("echoFeedback", 0x4e, 0xb3);
            Field("echoVolumeLeft").SetValue(player, (ushort)0x3567);
            Field("echoVolumeRight").SetValue(player, (ushort)0xa912);
            direct[0x61] = 0x35;
            direct[0x63] = 0xa9;
            Field("echoStoredTime").SetValue(player, stored);
            Field("echoDelay").SetValue(player, (byte)2);
            // Seed all destinations so skipped echo writes are observable.
            for (int index = 0; index < 10; index++)
                dsp.WriteRegister(rom.ReadByte(0xcf8a5a + index), 0x55);
            byte[] expected = new byte[128];
            for (int address = 0; address < 128; address++) expected[address] = dsp.ReadRegister((byte)address);
            // Native descending map: the first five writes are unconditional,
            // then FLG, then the four echo values once the delay has settled.
            int last = (stored & 0x80) != 0 ? 5 : stored == 2 ? 0 : 4;
            for (int index = 9; index >= last; index--)
                expected[rom.ReadByte(0xcf8a5a + index)] = direct[rom.ReadByte(0xcf8a64 + index)];
            var method = typeof(ManagedSpcPlayer).GetMethod("LoopPartOne", flags)!;
            method.CreateDelegate<Action>(player)();
            for (int address = 0; address < 128; address++)
                AssertEqual(expected[address], dsp.ReadRegister((byte)address), "native DSP publication map and echo gating");
            AssertEqual((byte)0, (byte)Field("keyOff").GetValue(player)!, "pending key-off clears after publication");
            AssertEqual((byte)0, (byte)Field("keyOn").GetValue(player)!, "pending key-on clears after publication");

            FieldInfo Field(string name) => typeof(ManagedSpcPlayer).GetField(name, flags)
                ?? throw new InvalidOperationException($"Missing SPC field {name}.");
            void Set(string name, int address, byte value)
            {
                direct[address] = value;
                Field(name).SetValue(player, value);
            }
        }
    }
}
