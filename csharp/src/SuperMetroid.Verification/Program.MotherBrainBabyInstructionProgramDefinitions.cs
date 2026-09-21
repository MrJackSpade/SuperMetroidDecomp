using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainBabyInstructionProgramDefinitions()
    {
        VerifyMotherBrainBabyInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyMotherBrainBabyInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MotherBrainBabyInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MotherBrainBabyInstructionMechanicsWord definition =
                MotherBrainBabyInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadMotherBrainBabyInstructionWord(
                    rom,
                    0xa90000 | definition.Address),
                $"Mother Brain Baby instruction mechanics word $A9:{definition.Address:X4}");
        }

        for (int index = 0;
             index < MotherBrainBabyInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                MotherBrainBabyInstructionProgramDefinitions.PresentationWordAddress(index);
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
            "adjacent Mother Brain Baby movement code is rejected as mechanics");

        _ = ProbeMotherBrainBabyInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMotherBrainBabyInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Mother Brain Baby allocation probe consumes live data");
        AssertEqual(
            0L,
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Mother Brain Baby mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Mother Brain Baby instruction mechanics: 12 compiled words across three " +
            "production programs; 9 spritemap reads remain presentation data.");
    }

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

    private static ushort ReadMotherBrainBabyInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
