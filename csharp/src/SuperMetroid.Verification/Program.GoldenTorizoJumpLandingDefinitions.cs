using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies the four Golden Torizo landing lists' control words match bank-$AA data and that their compiled byte range excludes the following opcode.</summary>
    /// <param name="rom">Address space supplying the native bank-$AA instruction words.</param>
    private static void VerifyGoldenTorizoJumpLandingDefinitions(ISnesAddressSpace rom)
    {
        const int bank = 0xaa0000;
        var addresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoJumpLandingInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                GoldenTorizoJumpLandingInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(addresses.Add(word.Address),
                $"Golden Torizo landing control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoJumpLandingInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoJumpLandingInstructionProgramDefinitions.End,
                $"Golden Torizo landing control $AA:{word.Address:X4} is bounded");
            ushort native = (ushort)(rom.ReadByte(bank | word.Address) |
                rom.ReadByte(bank | unchecked((ushort)(word.Address + 1))) << 8);
            AssertEqual(native, word.Value,
                $"Golden Torizo landing control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoJumpLandingInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo landing control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoJumpLandingInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(bank | word.Address) &&
                       GoldenTorizoJumpLandingInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(bank |
                        unchecked((ushort)(word.Address + 1))),
                $"Golden Torizo landing control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(20, addresses.Count,
            "Golden Torizo four landing lists contain 20 control words");
        AssertTrue(!GoldenTorizoJumpLandingInstructionProgramDefinitions
                .IsCompiledMechanicsByte(0xaa0000 | 0xcdd7),
            "Golden Torizo landing catalog does not claim the next opcode");
    }
}
