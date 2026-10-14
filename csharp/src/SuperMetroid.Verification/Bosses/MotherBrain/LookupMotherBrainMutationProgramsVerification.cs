using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks all 22 Mother Brain fake-death PLM lists against bank-$84 control and draw words.</summary>
internal static class LookupMotherBrainMutationProgramsVerification
{
    /// <summary>Compares duration and deletion controls with the retail fake-death lists.</summary>
    internal static void VerifyMotherBrainMutationProgramControls(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainMutationProgramField), () => VerifyMotherBrainMutationProgramField(rom, false));

    /// <summary>Compares the lists' interleaved draw operands with their retail words.</summary>
    internal static void VerifyMotherBrainMutationProgramDraws(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainMutationProgramField), () => VerifyMotherBrainMutationProgramField(rom, true));

    /// <summary>Checks complete word ownership, including two unused rows, before comparing the chosen field kind.</summary>
    private static void VerifyMotherBrainMutationProgramField(ISnesAddressSpace rom, bool draw)
    {
        ushort[] starts = [0xac05,0xac0b,0xac11,0xac17,0xac1d,0xac23,0xac29,0xac2f,
            0xac35,0xac3b,0xac41,0xac47,0xac4d,0xac53,0xac59,0xac5f,0xac65,0xac6b,
            0xac71,0xac77,0xac7d,0xac83];
        ushort[] controls = starts.SelectMany(start => new ushort[] {start, (ushort)(start + 4)}).ToArray();
        ushort[] operands = starts.Select(start => (ushort)(start + 2)).ToArray();
        AssertEqual(starts.Length, MotherBrainFakeDeathPlmProgramDefinitions.ProgramCount, "Mother Brain original list count");
        AssertTrue(controls.Concat(operands).Order().SequenceEqual(MotherBrainFakeDeathPlmProgramDefinitions.NativeWordAddresses()),
            "Mother Brain mutation original word enumeration");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool owned = controls.Contains(address) || operands.Contains(address);
            AssertEqual(owned, MotherBrainFakeDeathPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort actual),
                "Mother Brain mutation complete word ownership");
            if (!owned) AssertEqual((ushort)0, actual, "Mother Brain mutation missing word is zero");
            else if ((draw ? operands : controls).Contains(address))
            {
                ushort expected = (ushort)(SnesLittleEndian.Word(MotherBrainMutationProgramFieldRomByte(rom, 0x840000 | raw), MotherBrainMutationProgramFieldRomByte(rom, 0x840000 | (raw + 1))));
                AssertEqual(expected, actual, "Mother Brain mutation original field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Mother Brain shared reader ownership");
                AssertEqual(expected, shared, "Mother Brain shared reader original field");
            }
        }
    }

    /// <summary>Reads one original bank-$84 byte for the mutation-list comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain mutation program field")]
    private static byte MotherBrainMutationProgramFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
