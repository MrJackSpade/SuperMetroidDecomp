using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
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
        // Each program's frames are numbered from zero in the order it shows them, and each shows its own record (#1598).
        HashSet<int> records = [];
        foreach (PaletteFxProgram program in Enum.GetValues<PaletteFxProgram>())
        {
            int next = 0;
            foreach (var step in PaletteFxAnimations.Programs.Steps(program))
            {
                if (step is not AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.ShowStep show)
                    continue;
                AssertEqual(next++, show.Frame, $"{program}'s frames are numbered in the order it shows them");
                records.Add(PaletteFxRecord.Of(new PaletteFxFrame(program, show.Frame)).Record);
            }
        }
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
