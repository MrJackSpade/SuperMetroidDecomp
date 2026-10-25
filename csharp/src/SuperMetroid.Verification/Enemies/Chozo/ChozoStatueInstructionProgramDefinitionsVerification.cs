using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
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

    /// <summary>Checks the statues' typed programs, cues and shown frames against the four cartridge lists.</summary>
    internal static void VerifyChozoStatueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(ChozoStatueAnimationsVerification.VerifyChozoStatueAnimations), () => ChozoStatueAnimationsVerification.VerifyChozoStatueAnimations(rom));
        Console.WriteLine("Chozo statue: idle and activation programs with their cues match the cartridge.");
    }

    /// <summary>Runs the focused Chozo statue instruction mechanics check.</summary>
    [VerificationCommand("--chozo-statue-instruction-mechanics")]
    internal static int ChozoStatueInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions());
        return 0;
    }
}
