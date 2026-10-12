using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Represents mama turtle instruction program definitions verification.</summary>
internal static class MamaTurtleInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies mama turtle instruction program definitions.</summary>
    internal static void VerifyMamaTurtleInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies mama turtle instruction program definitions.</summary>
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

    /// <summary>Probe mama turtle instruction mechanics allocation.</summary>
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

    /// <summary>Mama turtle instruction program definitions command.</summary>
    [VerificationCommand("--mama-turtle-instruction-program-definitions")]
    internal static int MamaTurtleInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions());
        return 0;
    }
}
