using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Chozo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Chozo statue control words and separation from spritemap operands.</summary>
internal static class ChozoStatueInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the Chozo instruction mechanics comparison.</summary>
    internal static void VerifyChozoStatueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares 166 control words with ROM, rejects visual operands, and checks warmed lookup allocations.</summary>
    internal static void VerifyChozoStatueInstructionProgramDefinitions(
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
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaa0000 | definition.Address)),
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

    /// <summary>Repeatedly resolves a production instruction word to expose per-lookup allocation.</summary>
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

    /// <summary>Runs the focused Chozo statue instruction mechanics check.</summary>
    [VerificationCommand("--chozo-statue-instruction-mechanics")]
    internal static int ChozoStatueInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions());
        return 0;
    }
}
