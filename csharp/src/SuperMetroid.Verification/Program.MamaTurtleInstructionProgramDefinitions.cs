using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Mama Turtle instruction-layout checks against the retail ROM loaded from the working directory.</summary>
    private static void VerifyMamaTurtleInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled Mama Turtle mechanics and presentation addresses with the retail bank-$A2 instruction lists.</summary>
    /// <param name="rom">Retail ROM address space used to confirm each compiled mechanics word's stored value.</param>
    private static void VerifyMamaTurtleInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(117, MamaTurtleInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Mama Turtle compiled mechanics word count");
        AssertEqual(75, MamaTurtleInstructionProgramDefinitions.PresentationWordCount,
            "Mama Turtle live presentation word count");

        ushort previous = 0;
        for (int index = 0;
             index < MamaTurtleInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MamaTurtleInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertTrue(index == 0 || definition.Address > previous,
                $"Mama Turtle mechanics word {index} is strictly ordered");
            AssertEqual(definition.Value,
                ReadMamaTurtleInstructionWord(rom, definition.Address),
                $"Mama Turtle mechanics word $A2:{definition.Address:X4}");
            AssertTrue(
                MamaTurtleInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(
                    0xa20000 | definition.Address),
                $"Mama Turtle mechanics low byte $A2:{definition.Address:X4} is guarded");
            AssertTrue(
                MamaTurtleInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(
                    0xa20000 | unchecked((ushort)(definition.Address + 1))),
                $"Mama Turtle mechanics high byte $A2:{definition.Address + 1:X4} is guarded");
            previous = definition.Address;
        }

        var presentation = new HashSet<ushort>();
        for (int index = 0;
             index < MamaTurtleInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                MamaTurtleInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(presentation.Add(address),
                $"Mama Turtle presentation word $A2:{address:X4} is unique");
            AssertTrue(!MamaTurtleInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(
                    0xa20000 | address),
                $"Mama Turtle spritemap word $A2:{address:X4} remains live");
            AssertThrows<InvalidDataException>(
                () => MamaTurtleInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Mama Turtle spritemap word $A2:{address:X4} is rejected as mechanics");
        }

        AssertThrows<InvalidDataException>(
            () => MamaTurtleInstructionProgramDefinitions.ReadMechanicsWord(
                MamaTurtleInstructionProgramDefinitions.AdjacentMovementDefinitions),
            "adjacent Mama Turtle movement definitions are rejected as mechanics");

        _ = ProbeMamaTurtleInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMamaTurtleInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Mama Turtle allocation probe consumes compiled data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Mama Turtle mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Mama Turtle instruction definitions: 117 native control words, " +
            "byte ownership, 75 presentation addresses, bounds and allocation checks pass.");
    }

    /// <summary>Consumes repeated compiled mechanics lookups so the caller can measure their warmed allocation cost.</summary>
    /// <returns>Checksum accumulated from the selected instruction values to keep the lookup results observable.</returns>
    private static int ProbeMamaTurtleInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MamaTurtleInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MamaTurtleInstructionProgramDefinitions.BabyCrawlingLeft
                    : MamaTurtleInstructionProgramDefinitions.MamaLeaveShellRight);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word directly from bank $A2 in the supplied ROM.</summary>
    /// <param name="source">ROM address space containing the native Mama Turtle instruction bytes.</param>
    /// <param name="address">Bank-$A2 offset of the word's low byte.</param>
    /// <returns>The two consecutive bytes combined with the low byte in the least-significant position.</returns>
    private static ushort ReadMamaTurtleInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));
}
