using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>
/// #1298, #1739: the colour effects' typed animations, each matched step by step to the native bank-$8D lists it plays:
/// every timed record's duration, address and layout, every command and its operand, and every branch.
/// </summary>
internal static class PaletteFxProgramsVerification
{
    /// <summary>Every native list matches the steps it is in each animation, and every animation its lists' control flow.</summary>
    internal static void VerifyPaletteFxPrograms(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertTrue(Enum.GetValues<ColourEffectAnimation>().All(animation => PaletteFxNativeListAddresses.Entries.Any(entry => entry.Animation == animation && entry.FirstStep == 0)),
            "every typed colour effect animation starts at a native list");
        Dictionary<ushort, PaletteFxNativeList> lists = PaletteFxNativeProgramOracle.Lists(cartridge);
        // Every animation plays the phases its native lists' control flow plays (#1600).
        PaletteFxNativeProgramOracle.VerifyAnimations(lists);
        // Each animation's frames are numbered from zero in the order it shows them, and each shows a record (#1598).
        HashSet<int> records = [];
        foreach (ColourEffectAnimation animation in Enum.GetValues<ColourEffectAnimation>())
        {
            int next = 0;
            foreach (AnimationBeat<int, ColourEffectCue> beat in ColourEffectAnimations.Steps.Beats(animation))
            {
                if (beat is not AnimationBeat<int, ColourEffectCue>.Frame frame)
                    continue;
                AssertEqual(next++, frame.Shown, $"{animation}'s frames are numbered in the order it shows them");
                records.Add(PaletteFxRecord.Of(new ColourEffectFrame(animation, frame.Shown)).Record);
            }
        }
        AssertEqual(PaletteFxRecordDefinitionsConstants.RecordCount, records.Count, "every colour record is shown by an animation");
        Console.WriteLine($"  Colour effect animations: {lists.Count} native lists ({lists.Values.Sum(list => list.BeatAddresses.Length)} beats, " +
            $"{records.Count} colour records) and {Enum.GetValues<ColourEffectAnimation>().Length} animations match bank $8D.");
    }

    /// <summary>Runs the palette-FX program oracle.</summary>
    [VerificationCommand("--palette-fx-programs")]
    internal static int PaletteFxProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPaletteFxPrograms), () => VerifyPaletteFxPrograms(rom));
        return 0;
    }
}
