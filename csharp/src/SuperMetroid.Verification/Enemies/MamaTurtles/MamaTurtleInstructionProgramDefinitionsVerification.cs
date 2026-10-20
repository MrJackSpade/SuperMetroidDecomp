using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Enemies.MamaTurtles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Mama Turtle's and baby turtles' typed animation programs.</summary>
internal static class MamaTurtleInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for Mama Turtle instruction-definition checks.</summary>
    internal static void VerifyMamaTurtleInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the turtles' typed programs, cues and shown frames against the cartridge lists.</summary>
    internal static void VerifyMamaTurtleInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(MamaTurtleAnimationsVerification.VerifyMamaTurtleAnimations), () => MamaTurtleAnimationsVerification.VerifyMamaTurtleAnimations(rom));
        Console.WriteLine("Mama Turtle: crawl, spin, shell and sleep programs with their cues match the cartridge.");
    }

    /// <summary>Runs the Mama Turtle instruction-definition verification command.</summary>
    [VerificationCommand("--mama-turtle-instruction-program-definitions")]
    internal static int MamaTurtleInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions());
        return 0;
    }
}
