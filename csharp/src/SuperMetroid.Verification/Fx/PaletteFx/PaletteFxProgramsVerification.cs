using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
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
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyPaletteFxPrograms(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<PaletteFxSegment>().Length, PaletteFxNativeListAddresses.Entries.Length, "every typed palette-FX program has a native entry");
        Dictionary<PaletteFxSegment, PaletteFxNativeList> lists = PaletteFxNativeProgramOracle.Lists(cartridge);
        // Every animation plays the phases its native lists' control flow plays (#1600).
        PaletteFxNativeProgramOracle.VerifyAnimations(lists);
        // Each segment's frames are numbered from zero in the order it shows them, and each shows its own record (#1598).
        HashSet<int> records = [];
        foreach (PaletteFxSegment segment in Enum.GetValues<PaletteFxSegment>())
        {
            int next = 0;
            foreach (AnimationBeat<int, PaletteFxCue> beat in PaletteFxSegments.All.Beats(segment))
            {
                if (beat is not AnimationBeat<int, PaletteFxCue>.Frame frame)
                    continue;
                AssertEqual(next++, frame.Shown, $"{segment}'s frames are numbered in the order it shows them");
                records.Add(PaletteFxRecord.Of(new PaletteFxFrame(segment, frame.Shown)).Record);
            }
        }
        AssertEqual(PaletteFxRecordDefinitionsConstants.RecordCount, records.Count, "every colour record is shown by a segment");
        Console.WriteLine($"  Palette-FX animations: {lists.Count} segments ({lists.Values.Sum(list => list.BeatAddresses.Length)} beats, " +
            $"{records.Count} colour records) and {PaletteFxAnimations.StartingAt.Count} animations match bank $8D.");
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
