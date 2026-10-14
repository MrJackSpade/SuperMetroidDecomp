using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Programs;

/// <summary>Checks compiled Golden Torizo landing control words against their bank-$AA source.</summary>
internal static class GoldenTorizoJumpLandingDefinitionsVerification
{
    /// <summary>Checks uniqueness, native values, two-byte ownership, and the boundary after the four landing lists.</summary>
    internal static void VerifyGoldenTorizoJumpLandingDefinitions(ISnesAddressSpace rom)
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
            ushort native = (ushort)(SnesLittleEndian.Word(GoldenTorizoJumpLandingDefinitionsRomByte(rom, bank | word.Address), GoldenTorizoJumpLandingDefinitionsRomByte(rom, bank | unchecked((ushort)(word.Address + 1)))));
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

    /// <summary>Reads a native byte for the landing-list comparison only.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Golden Torizo jump landing definitions")]
    private static byte GoldenTorizoJumpLandingDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
