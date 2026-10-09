using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Chozo-statue mechanics audit against the retail ROM loaded from the current directory.</summary>
    private static void VerifyChozoStatueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled Chozo mechanics words with cartridge data and checks selector separation and lookup allocation.</summary>
    /// <param name="rom">The loaded retail address space used as the source of native instruction words.</param>
    private static void VerifyChozoStatueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ChozoStatueInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
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
            "programs; 52 spritemap selectors bind separately installed presentation data.");
    }

    /// <summary>Warms and repeats a fixed mechanics-word lookup to expose per-call allocations to the audit.</summary>
    /// <returns>A checksum of the repeated word values, ensuring the reads contribute to observable work.</returns>
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

    /// <summary>Reads one little-endian instruction word from the retail address space.</summary>
    /// <param name="bus">The retail ROM address space.</param>
    /// <param name="address">The full SNES address of the word's low byte.</param>
    /// <returns>The word assembled from the addressed byte and its following byte.</returns>
    private static ushort ReadChozoStatueInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
