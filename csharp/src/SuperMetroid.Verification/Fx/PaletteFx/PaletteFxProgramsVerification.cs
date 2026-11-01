using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>
/// #1298: the palette-FX objects' typed programs, each matched step by step to its native bank-$8D list: every timed
/// record's duration, address and layout, every command and its operand, and every branch.
/// </summary>
internal static class PaletteFxProgramsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPaletteFxPrograms(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<PaletteFxProgram>().Length, PaletteFxNativeListAddresses.Entries.Length, "every typed palette-FX program has a native entry");
        Dictionary<PaletteFxProgram, ushort[]> steps = PaletteFxNativeProgramOracle.StepAddresses(cartridge);
        HashSet<int> records = [];
        foreach (PaletteFxProgram program in Enum.GetValues<PaletteFxProgram>())
            foreach (var step in PaletteFxAnimations.Programs.Steps(program))
                if (step is AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep show)
                    records.Add(show.Frame.Record);
        AssertEqual(PaletteFxRecordDefinitionsConstants.RecordCount, records.Count, "every colour record is shown by a program");
        Console.WriteLine($"  Palette-FX programs: {steps.Count} programs ({steps.Values.Sum(s => s.Length)} steps, {records.Count} colour records) match bank $8D.");
    }

    /// <summary>Runs the palette-FX program oracle.</summary>
    [VerificationCommand("--palette-fx-programs")]
    internal static int PaletteFxProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPaletteFxPrograms), () => VerifyPaletteFxPrograms(rom));
        return 0;
    }
}
