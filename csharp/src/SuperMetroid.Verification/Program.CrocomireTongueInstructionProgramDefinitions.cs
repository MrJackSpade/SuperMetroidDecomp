using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Crocomire tongue instruction-program checks against the retail ROM.</summary>
    private static void VerifyCrocomireTongueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCrocomireTongueInstructionProgramDefinitions), () => VerifyCrocomireTongueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Validates the compiled mechanics and presentation maps, dispatch behavior, and warmed lookup allocation.</summary>
    /// <param name="rom">Retail ROM address space used as the independent expected-data source.</param>
    private static void VerifyCrocomireTongueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCrocomireTongueMechanicsDispatch), () => VerifyCrocomireTongueMechanicsDispatch(rom));
        Suite(nameof(VerifyCrocomireTonguePresentationPositions), () => VerifyCrocomireTonguePresentationPositions(rom));

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

    /// <summary>Checks the fourteen mechanics words and verifies that byte dispatch excludes data gaps and presentation selectors.</summary>
    /// <param name="rom">Retail ROM used to read the expected instruction words.</param>
    private static void VerifyCrocomireTongueMechanicsDispatch(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
        [
            0xbe56, 0xbe5a, 0xbe5e, 0xbe62, 0xbe66, 0xbe68, 0xbf62,
            0xbf98, 0xbf9c, 0xbfa0, 0xbfa4, 0xbfa8, 0xbfac, 0xbfae,
        ];
        AssertEqual(addresses.Length, CrocomireTongueInstructionProgramDefinitionsTooling.MechanicsWordCount, "tongue mechanics count");
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort address = addresses[i];
            var definition = CrocomireTongueInstructionProgramDefinitionsTooling.MechanicsWord(i);
            ushort expected = ReadCrocomireTongueInstructionWord(rom, 0xa40000 | address);
            AssertEqual(address, definition.Address, "tongue independent mechanics address");
            AssertEqual(expected, definition.Value, "tongue enumerated native control value");
            AssertEqual(expected, CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(address), "tongue native dispatch value");
            AssertThrows<InvalidDataException>(() => CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord((ushort)(address + 1)), "tongue odd word rejected");
        }
        // Verify the exact byte classifier domain against the independent original
        // address set, including holes between programs and presentation operands.
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = Array.Exists(addresses, word => address == word || address == word + 1);
            AssertEqual(expected, CrocomireTongueInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa40000 | address), "tongue mechanics byte domain");
        }
        AssertTrue(!CrocomireTongueInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa5be56), "tongue rejects another bank");
        foreach (ushort address in new ushort[] { 0, 0xbe54, 0xbe58, 0xbe6a, 0xbf60, 0xbf64, 0xbf96, 0xbf9a, 0xbfb0, 0xffff })
            AssertThrows<InvalidDataException>(() => CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(address), "tongue rejects nonmechanics address");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitionsTooling.MechanicsWord(-1), "tongue negative mechanics index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitionsTooling.MechanicsWord(14), "tongue mechanics index past end");
    }

    /// <summary>Checks the nine spritemap operand addresses and their values against the cartridge.</summary>
    /// <param name="rom">Retail ROM providing the expected presentation words.</param>
    private static void VerifyCrocomireTonguePresentationPositions(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xbe58, 0xbe5c, 0xbe60, 0xbe64, 0xbf9a, 0xbf9e, 0xbfa2, 0xbfa6, 0xbfaa];
        AssertEqual(addresses.Length, CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordCount, "tongue presentation count");
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort actual = CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordAddress(i);
            AssertEqual(addresses[i], actual, "tongue independent presentation address");
            AssertEqual(ReadCrocomireTongueInstructionWord(rom, 0xa40000 | addresses[i]),
                ReadCrocomireTongueInstructionWord(rom, 0xa40000 | actual), "tongue original presentation operand");
            AssertThrows<InvalidDataException>(() => CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(actual), "tongue presentation is not mechanics");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordAddress(-1), "tongue negative presentation index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordAddress(9), "tongue presentation index past end");
    }
    /// <summary>Measures warmed mechanics-lookup allocation while consuming repeated fight-word reads.</summary>
    /// <returns>The checksum accumulated from the repeated instruction-word reads.</returns>
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

    /// <summary>Reads an instruction word from two consecutive cartridge bytes in little-endian order.</summary>
    /// <param name="bus">Address space supplying the instruction bytes.</param>
    /// <param name="address">Byte address of the word's low-order byte.</param>
    /// <returns>The decoded 16-bit instruction word.</returns>
    private static ushort ReadCrocomireTongueInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
