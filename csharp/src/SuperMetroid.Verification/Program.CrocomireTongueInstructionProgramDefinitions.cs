using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireTongueInstructionProgramDefinitions()
    {
        VerifyCrocomireTongueInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyCrocomireTongueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < CrocomireTongueInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CrocomireTongueInstructionMechanicsWord definition =
                CrocomireTongueInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadCrocomireTongueInstructionWord(rom, 0xa40000 | definition.Address),
                $"Crocomire tongue mechanics word $A4:{definition.Address:X4}");
        }

        for (int index = 0;
             index < CrocomireTongueInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                CrocomireTongueInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertThrows<InvalidDataException>(
                () => CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Crocomire tongue spritemap $A4:{address:X4} is rejected as mechanics");
        }

        AssertThrows<InvalidDataException>(
            () => CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(0xbe6a),
            "unused reversed Crocomire tongue list is rejected as mechanics");

        _ = ProbeCrocomireTongueInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCrocomireTongueInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Crocomire tongue allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Crocomire tongue mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Crocomire tongue instruction mechanics: fourteen compiled words, complete " +
            "fight/melting loops and terminal sleep; nine spritemap reads remain " +
            "presentation data.");
    }

    private static int ProbeCrocomireTongueInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(
                CrocomireTongueInstructionProgramDefinitions.Fight);
        }
        return checksum;
    }

    private static ushort ReadCrocomireTongueInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
