using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireTongueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCrocomireTongueInstructionProgramDefinitions), () => VerifyCrocomireTongueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

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

    private static void VerifyCrocomireTongueMechanicsDispatch(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
        [
            0xbe56, 0xbe5a, 0xbe5e, 0xbe62, 0xbe66, 0xbe68, 0xbf62,
            0xbf98, 0xbf9c, 0xbfa0, 0xbfa4, 0xbfa8, 0xbfac, 0xbfae,
        ];
        AssertEqual(addresses.Length, CrocomireTongueInstructionProgramDefinitions.MechanicsWordCount, "tongue mechanics count");
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort address = addresses[i];
            var definition = CrocomireTongueInstructionProgramDefinitions.MechanicsWord(i);
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
            AssertEqual(expected, CrocomireTongueInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa40000 | address), "tongue mechanics byte domain");
        }
        AssertTrue(!CrocomireTongueInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa5be56), "tongue rejects another bank");
        foreach (ushort address in new ushort[] { 0, 0xbe54, 0xbe58, 0xbe6a, 0xbf60, 0xbf64, 0xbf96, 0xbf9a, 0xbfb0, 0xffff })
            AssertThrows<InvalidDataException>(() => CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(address), "tongue rejects nonmechanics address");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitions.MechanicsWord(-1), "tongue negative mechanics index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitions.MechanicsWord(14), "tongue mechanics index past end");
    }

    private static void VerifyCrocomireTonguePresentationPositions(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xbe58, 0xbe5c, 0xbe60, 0xbe64, 0xbf9a, 0xbf9e, 0xbfa2, 0xbfa6, 0xbfaa];
        AssertEqual(addresses.Length, CrocomireTongueInstructionProgramDefinitions.PresentationWordCount, "tongue presentation count");
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort actual = CrocomireTongueInstructionProgramDefinitions.PresentationWordAddress(i);
            AssertEqual(addresses[i], actual, "tongue independent presentation address");
            AssertEqual(ReadCrocomireTongueInstructionWord(rom, 0xa40000 | addresses[i]),
                ReadCrocomireTongueInstructionWord(rom, 0xa40000 | actual), "tongue original presentation operand");
            AssertThrows<InvalidDataException>(() => CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(actual), "tongue presentation is not mechanics");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitions.PresentationWordAddress(-1), "tongue negative presentation index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitions.PresentationWordAddress(9), "tongue presentation index past end");
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
