using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMamaTurtleInstructionProgramDefinitions()
    {
        VerifyMamaTurtleInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyMamaTurtleInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(117, MamaTurtleInstructionProgramDefinitions.MechanicsWordCount,
            "Mama Turtle compiled mechanics word count");
        AssertEqual(75, MamaTurtleInstructionProgramDefinitions.PresentationWordCount,
            "Mama Turtle live presentation word count");

        ushort previous = 0;
        for (int index = 0;
             index < MamaTurtleInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MamaTurtleInstructionMechanicsWord definition =
                MamaTurtleInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(index == 0 || definition.Address > previous,
                $"Mama Turtle mechanics word {index} is strictly ordered");
            AssertEqual(definition.Value,
                ReadMamaTurtleInstructionWord(rom, definition.Address),
                $"Mama Turtle mechanics word $A2:{definition.Address:X4}");
            AssertTrue(
                MamaTurtleInstructionProgramDefinitions.IsCompiledMechanicsByte(
                    0xa20000 | definition.Address),
                $"Mama Turtle mechanics low byte $A2:{definition.Address:X4} is guarded");
            AssertTrue(
                MamaTurtleInstructionProgramDefinitions.IsCompiledMechanicsByte(
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
            AssertTrue(!MamaTurtleInstructionProgramDefinitions.IsCompiledMechanicsByte(
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
            "Mama Turtle instruction mechanics: 117 compiled words, all thirteen " +
            "parent/child programs, nine callbacks, and 75 live spritemap reads pass.");
    }

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

    private static ushort ReadMamaTurtleInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));
}
