using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies that compiled Mother Brain mutation control words match the cartridge and shared reader.</summary>
    /// <param name="rom">Retail address space supplying the original PLM instruction words.</param>
    private static void VerifyMotherBrainMutationProgramControls(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainMutationProgramField), () => VerifyMotherBrainMutationProgramField(rom, false));

    /// <summary>Verifies that compiled Mother Brain mutation draw operands match the cartridge and shared reader.</summary>
    /// <param name="rom">Retail address space supplying the original PLM draw operands.</param>
    private static void VerifyMotherBrainMutationProgramDraws(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainMutationProgramField), () => VerifyMotherBrainMutationProgramField(rom, true));

    /// <summary>Checks address ownership for all mutation lists and compares the selected control or draw words with retail data.</summary>
    /// <param name="rom">Retail address space used as the reference for each compiled word.</param>
    /// <param name="draw"><see langword="true"/> to compare draw operands; otherwise compares control words.</param>
    private static void VerifyMotherBrainMutationProgramField(ISnesAddressSpace rom, bool draw)
    {
        // Independent original list starts, including the two unused row lists.
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
                ushort expected = (ushort)(rom.ReadByte(0x840000 | raw) | rom.ReadByte(0x840000 | (raw + 1)) << 8);
                AssertEqual(expected, actual, "Mother Brain mutation original field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Mother Brain shared reader ownership");
                AssertEqual(expected, shared, "Mother Brain shared reader original field");
            }
        }
    }
}
