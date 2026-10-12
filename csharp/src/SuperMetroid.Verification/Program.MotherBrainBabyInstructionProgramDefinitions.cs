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

/// <summary>Represents mother brain baby instruction program definitions verification.</summary>
internal static class MotherBrainBabyInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies mother brain baby instruction program definitions.</summary>
    internal static void VerifyMotherBrainBabyInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMotherBrainBabyInstructionProgramDefinitions), () => VerifyMotherBrainBabyInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies mother brain baby instruction program definitions.</summary>
    internal static void VerifyMotherBrainBabyInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MotherBrainBabyInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MotherBrainBabyInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | definition.Address)),
                $"Mother Brain Baby instruction mechanics word $A9:{definition.Address:X4}");
        }

        for (int index = 0;
             index < MotherBrainBabyInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                MotherBrainBabyInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertThrows<InvalidDataException>(
                () => MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Mother Brain Baby spritemap $A9:{address:X4} is rejected as mechanics");
        }

        AssertThrows<InvalidDataException>(
            () => MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(
                (ushort)BabyMetroidInstruction.GotoInitial),
            "Mother Brain Baby initial-goto callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(
                (ushort)BabyMetroidInstruction.GotoDrainingMotherBrain),
            "Mother Brain Baby draining-goto callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(
                MotherBrainBabyInstructionProgramDefinitions.FirstAdjacentMovementCode),
            "adjacent Mother Brain palette code is rejected as mechanics");

        _ = ProbeMotherBrainBabyInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMotherBrainBabyInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Mother Brain Baby allocation probe consumes live data");
        AssertEqual(
            0L,
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Mother Brain Baby mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Mother Brain Baby instruction mechanics: 12 native-word comparisons, nine visual-operand " +
            "rejections, callback/adjacent-code rejection and allocation checks pass.");
    }

    /// <summary>Probe mother brain baby instruction mechanics allocation.</summary>
    private static int ProbeMotherBrainBabyInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(
                MotherBrainBabyInstructionProgramDefinitions.Initial);
        }
        return checksum;
    }

    /// <summary>Mother brain baby instruction mechanics command.</summary>
    [VerificationCommand("--mother-brain-baby-instruction-mechanics")]
    internal static int MotherBrainBabyInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMotherBrainBabyInstructionProgramDefinitions), () => VerifyMotherBrainBabyInstructionProgramDefinitions());
        return 0;
    }
}
