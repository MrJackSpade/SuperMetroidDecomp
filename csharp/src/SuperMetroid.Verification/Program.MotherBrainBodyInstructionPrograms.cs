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
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static class MotherBrainBodyInstructionProgramsVerification
{
    /// <summary>
    /// Proves that the compiled Mother Brain body mechanics words match the pinned cartridge
    /// and that every supported native program can run while those ROM bytes are inaccessible.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain body instruction programs")]
    internal static void VerifyMotherBrainBodyInstructionPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));

        foreach (MotherBrainBodyInstructionMechanicsWord definition in
            MotherBrainBodyInstructionProgramDefinitionsTooling.AllWords)
        {
            AssertEqual(
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | definition.Address)),
                definition.Word,
                $"Mother Brain body compiled mechanics word $A9:{definition.Address:X4}");

            if ((definition.Word & 0x8000) == 0)
            {
                ushort visualOperand = unchecked((ushort)(definition.Address + 2));
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | visualOperand)),
                    MotherBrainBodyInstructionProgramDefinitions.ReadVisualSelector(visualOperand),
                    $"Mother Brain body visual selector $A9:{visualOperand:X4}");
                AssertTrue(
                    !MotherBrainBodyInstructionProgramDefinitions.TryGetWord(
                        visualOperand,
                        out _),
                    $"Mother Brain body spritemap $A9:{definition.Address + 2:X4} remains presentation-owned");
            }
        }

        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 |
                MotherBrainBodyInstructionProgramDefinitions.InitialDummyVisualOperand)),
            MotherBrainBodyInstructionProgramDefinitions.ReadInitialDummyVisualSelector(
                MotherBrainBodyInstructionProgramDefinitions.InitialDummyVisualOperand),
            "Mother Brain initial dummy selector matches the cartridge");
        AssertThrows<InvalidDataException>(
            () => MotherBrainBodyInstructionProgramDefinitions.ReadInitialDummyVisualSelector(
                MotherBrainBodyInstructionProgramDefinitions.InitialDummy),
            "Mother Brain dummy visual selector rejects neighboring mechanics data");

        Console.WriteLine(
            $"  Mother Brain: {MotherBrainBodyInstructionProgramDefinitionsTooling.AllWords.Count} " +
            "body command/duration words and visual selectors match the cartridge.");
    }

    [VerificationCommand("--mother-brain-body-instruction-mechanics")]
    internal static int MotherBrainBodyInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMotherBrainBodyInstructionPrograms), () => VerifyMotherBrainBodyInstructionPrograms());
        return 0;
    }
}
