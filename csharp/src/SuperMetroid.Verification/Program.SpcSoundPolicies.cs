using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using System.Reflection;

internal static partial class Program
{
    private static void VerifySpcSoundPolicy1(ISnesAddressSpace rom) => VerifySpcSoundPolicy(rom, 0, 66, 0x1f4d, 0x03a1, 0x04bb);
    private static void VerifySpcSoundPolicy2(ISnesAddressSpace rom) => VerifySpcSoundPolicy(rom, 1, 127, 0x31b1, 0x0441, 0x04bc);
    private static void VerifySpcSoundPolicy3(ISnesAddressSpace rom) => VerifySpcSoundPolicy(rom, 2, 47, 0x4776, 0x0479, 0x04bd);

    private static void VerifySpcSoundPolicy(ISnesAddressSpace rom, int library, int count, int dispatch, int voicesAddress, int priorityAddress)
    {
        // Original SPC image is loaded from CF:6C08 + SPC address. These dispatch
        // tables and handler labels are independently identified in PJBoy/SM-SPC
        // 17b0c1ee46eed2f97909fbb6141a5ee6a693c45a vanilla/sound library N.asm.
        byte Read(int address) => rom.ReadByte(0xcf6c08 + address);
        int Word(int address) => Read(address) | Read(address + 1) << 8;
        MethodInfo configure = typeof(ManagedSpcPlayer).GetMethod("ConfigureSoundLibrary", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Missing production sound policy consumer.");
        var checkedVoicePolicies = new HashSet<byte>();
        for (int command = 1; command <= count; command++)
        {
            int handler = Word(dispatch + (command - 1) * 2);
            if (Read(handler) == 0x3f) // CALL absolute, followed by RET
            {
                AssertEqual((byte)0x6f, Read(handler + 3), "native dispatch wrapper return");
                handler = Word(handler + 1);
            }
            byte originalPolicy = handler switch
            {
                0x2aab or 0x3987 or 0x4dd0 => 0,
                0x2ab6 or 0x3992 or 0x4de0 => 1,
                0x2ac1 or 0x399d or 0x4e63 => 2,
                0x2acc or 0x39a8 or 0x4e6e => 3,
                0x2ad7 or 0x4e79 => 4,
                0x2ae2 or 0x4e84 => 5,
                _ => throw new InvalidOperationException($"Unexpected native policy handler {handler:X4}."),
            };
            byte actualPolicy = SpcSoundEffectTables.Configuration(library, command);
            AssertEqual(originalPolicy, actualPolicy, "original command policy identity");
            foreach (byte prior in new byte[] { 0, 0xa5 })
            {
                byte expectedVoices = 0, expectedPriority = prior, expectedMode = prior;
                int pc = handler;
                for (int write = 0; write < 3 && Read(pc) != 0x6f; write++, pc += 5)
                {
                    AssertEqual((byte)0xe8, Read(pc), "native MOV A immediate");
                    AssertEqual((byte)0xc5, Read(pc + 2), "native MOV absolute A");
                    byte value = Read(pc + 1);
                    int destination = Word(pc + 3);
                    if (destination == voicesAddress) expectedVoices = value;
                    else if (destination == priorityAddress) expectedPriority = value;
                    else if (library == 2 && destination == 0x04ba) expectedMode = value;
                    else throw new InvalidOperationException($"Unexpected native policy write {destination:X4}.");
                }
                AssertEqual((byte)0x6f, Read(pc), "native policy ends after bounded writes");
                var state = new ManagedSpcSoundLibrary { Priority = prior, Mode = prior };
                configure.Invoke(null, new object[] { library, state, actualPolicy });
                AssertEqual(expectedVoices, state.VoicesToSetup, "production policy voices");
                AssertEqual(expectedPriority, state.Priority, "production policy priority and preservation");
                AssertEqual(expectedMode, state.Mode, "production policy mode and preservation");
                if (checkedVoicePolicies.Add(originalPolicy))
                    AssertEqual((int)expectedVoices, SpcSoundEffectTables.GetVoiceCount(library, originalPolicy), "native policy channel-program count");
            }
        }
        int nativePolicyCount = library == 1 ? 4 : 6;
        for (int policy = 0; policy < nativePolicyCount; policy++)
            AssertTrue(checkedVoicePolicies.Contains((byte)policy), "every native voice policy has an original-handler proof");
        for (int invalid = nativePolicyCount; invalid <= byte.MaxValue; invalid++)
            AssertThrows<InvalidDataException>(() => SpcSoundEffectTables.GetVoiceCount(library, (byte)invalid), "unknown manifest policy byte");
        foreach (int invalid in new[] { int.MinValue, -1, 0, count + 1, 256, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SpcSoundEffectTables.Configuration(library, invalid), "unsupported sound command");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => SpcSoundEffectTables.Configuration(invalid, 1), "unsupported library");
            AssertThrows<ArgumentOutOfRangeException>(() => SpcSoundEffectTables.GetVoiceCount(invalid, 0), "unsupported voice-count library");
        }
    }
}
