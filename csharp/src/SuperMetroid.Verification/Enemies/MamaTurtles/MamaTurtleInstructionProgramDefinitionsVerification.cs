using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Enemies.MamaTurtles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mama Turtle's compiled instruction words and live visual-operand boundary.</summary>
internal static class MamaTurtleInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for Mama Turtle instruction-definition checks.</summary>
    internal static void VerifyMamaTurtleInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares ordered control words, byte ownership, visual exclusions, and allocation-free lookup.</summary>
    internal static void VerifyMamaTurtleInstructionProgramDefinitions(
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
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | definition.Address),
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

    /// <summary>Alternates two compiled program words to expose allocation in warmed lookup calls.</summary>
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

    /// <summary>Runs the Mama Turtle instruction-definition verification command.</summary>
    [VerificationCommand("--mama-turtle-instruction-program-definitions")]
    internal static int MamaTurtleInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions());
        return 0;
    }
}
