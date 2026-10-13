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

/// <summary>Checks compiled Crocomire tongue control words while keeping spritemap operands separate.</summary>
internal static class CrocomireTongueInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail oracle and runs the full tongue mechanics comparison.</summary>
    internal static void VerifyCrocomireTongueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCrocomireTongueInstructionProgramDefinitions), () => VerifyCrocomireTongueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks fourteen native mechanics words, nine visual operand positions, and allocation-free lookup.</summary>
    internal static void VerifyCrocomireTongueInstructionProgramDefinitions(
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

    /// <summary>Compares mechanics values and the complete bank-$A4 byte ownership domain.</summary>
    internal static void VerifyCrocomireTongueMechanicsDispatch(SuperMetroidAddressSpace rom)
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
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | address));
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

    /// <summary>Checks spritemap operand addresses and excludes them from mechanics dispatch.</summary>
    internal static void VerifyCrocomireTonguePresentationPositions(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xbe58, 0xbe5c, 0xbe60, 0xbe64, 0xbf9a, 0xbf9e, 0xbfa2, 0xbfa6, 0xbfaa];
        AssertEqual(addresses.Length, CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordCount, "tongue presentation count");
        for (int i = 0; i < addresses.Length; i++)
        {
            ushort actual = CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordAddress(i);
            AssertEqual(addresses[i], actual, "tongue independent presentation address");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | addresses[i])),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | actual)), "tongue original presentation operand");
            AssertThrows<InvalidDataException>(() => CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(actual), "tongue presentation is not mechanics");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordAddress(-1), "tongue negative presentation index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordAddress(9), "tongue presentation index past end");
    }
    /// <summary>Consumes repeated fight-list lookups to measure warmed per-frame allocation.</summary>
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

    /// <summary>Runs the Crocomire tongue program comparison with the retail cartridge.</summary>
    [VerificationCommand("--crocomire-tongue-instruction-mechanics")]
    internal static int CrocomireTongueInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCrocomireTongueInstructionProgramDefinitions), () => VerifyCrocomireTongueInstructionProgramDefinitions());
        return 0;
    }
}
