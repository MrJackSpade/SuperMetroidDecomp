using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and verifies the Mother Brain Baby instruction-mechanics catalog.</summary>
    private static void VerifyMotherBrainBabyInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMotherBrainBabyInstructionProgramDefinitions), () => VerifyMotherBrainBabyInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares catalog mechanics words with ROM data and checks that visual operands and callback addresses are rejected.</summary>
    /// <param name="rom">The ROM address space used to read the native instruction words.</param>
    private static void VerifyMotherBrainBabyInstructionProgramDefinitions(
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
                ReadMotherBrainBabyInstructionWord(
                    rom,
                    0xa90000 | definition.Address),
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
                MotherBrainInstructionCodes.Instruction_BabyMetroid_GotoInitial),
            "Mother Brain Baby initial-goto callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(
                MotherBrainInstructionCodes.Instruction_BabyMetroid_GotoDrainingMotherBrain),
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

    /// <summary>Warms the mechanics lookup and consumes repeated reads to verify that subsequent lookups allocate no per-frame storage.</summary>
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

    /// <summary>Reads one little-endian 16-bit instruction word from two consecutive bus addresses.</summary>
    /// <param name="bus">The address space containing the instruction bytes.</param>
    /// <param name="address">The address of the word's low byte.</param>
    private static ushort ReadMotherBrainBabyInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
