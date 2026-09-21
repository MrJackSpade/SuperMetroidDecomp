using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyChozoStatueInstructionProgramDefinitions()
    {
        VerifyChozoStatueInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyChozoStatueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ChozoStatueInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ChozoStatueInstructionMechanicsWord definition =
                ChozoStatueInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadChozoStatueInstructionWord(rom, 0xaa0000 | definition.Address),
                $"Chozo statue instruction mechanics word $AA:{definition.Address:X4}");
        }

        for (int index = 0;
             index < ChozoStatueInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                ChozoStatueInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertThrows<InvalidDataException>(
                () => ChozoStatueInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Chozo statue spritemap $AA:{address:X4} is rejected as mechanics");
        }

        AssertThrows<InvalidDataException>(
            () => ChozoStatueInstructionProgramDefinitions.ReadMechanicsWord(0xe429),
            "adjacent Chozo callback code is rejected as instruction mechanics");

        _ = ProbeChozoStatueInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeChozoStatueInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Chozo statue allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Chozo statue mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Chozo statue instruction mechanics: 166 compiled words across four production " +
            "programs; 52 spritemap reads remain presentation data.");
    }

    private static int ProbeChozoStatueInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ChozoStatueInstructionProgramDefinitions.ReadMechanicsWord(
                ChozoStatueInstructionProgramDefinitions.WreckedShipActivated);
        }
        return checksum;
    }

    private static ushort ReadChozoStatueInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
