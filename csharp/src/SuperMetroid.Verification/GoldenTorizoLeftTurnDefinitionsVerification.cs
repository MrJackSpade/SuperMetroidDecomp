using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Golden Torizo left-turn control words and reused facing-screen art against bank $AA.</summary>
internal static class GoldenTorizoLeftTurnDefinitionsVerification
{
    /// <summary>Compares twelve compiled controls and two visual operands with the retail program.</summary>
    internal static void VerifyGoldenTorizoLeftTurnDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = 0xaa;
        var mechanicsAddresses = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoLeftTurnInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                GoldenTorizoLeftTurnInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Golden Torizo left-turn control $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge &&
                       word.Address < GoldenTorizoLeftTurnInstructionProgramDefinitions.End,
                $"Golden Torizo left-turn control $AA:{word.Address:X4} is bounded");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo left-turn control $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoLeftTurnInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo left-turn control $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoLeftTurnInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, word.Address))) &&
                       GoldenTorizoLeftTurnInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, unchecked((ushort)(word.Address + 1))))),
                $"Golden Torizo left-turn control $AA:{word.Address:X4} owns both bytes");
        }
        AssertEqual(12, mechanicsAddresses.Count,
            "Golden Torizo left-turn control-word count");

        for (int index = 0;
             index < GoldenTorizoLeftTurnInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GoldenTorizoLeftTurnInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertEqual(GoldenTorizoLeftTurnInstructionProgramDefinitions.FacingScreenFrame,
                native,
                $"Golden Torizo left-turn selector $AA:{address:X4} reuses the facing-screen frame");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo left-turn selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Golden Torizo left-turn selector $AA:{address:X4} is not mechanics");
        }
        AssertEqual(2, GoldenTorizoLeftTurnInstructionProgramDefinitions.PresentationWordCount,
            "Golden Torizo left-turn visual occurrence count");
        AssertTrue(TorizoCollisionDefinitions.HasFrame(
                GoldenTorizoLeftTurnInstructionProgramDefinitions.FacingScreenFrame),
            "left-turn selectors reuse the right-turn physical frame owner");
        Console.WriteLine(
            "Golden Torizo left turn: 12 control words and two selectors of " +
            "the existing facing-screen frame match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(SnesLittleEndian.Word(GoldenTorizoLeftTurnDefinitionsRomByte(rom, (bank << 16) | address), GoldenTorizoLeftTurnDefinitionsRomByte(rom, (bank << 16) |
                    unchecked((ushort)(address + 1)))));
    }

    /// <summary>Reads a pinned bank-$AA program byte for the left-turn oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Golden Torizo left turn definitions")]
    private static byte GoldenTorizoLeftTurnDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
