using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Confirms Mother Brain's sheets hold her frames whole (#1513): each frame her leg pages or her corpse's pages complete
/// has one picture, the state once that run of pages has loaded, and shows it at every step it is drawn at and before the
/// first; none pictures a page still loading, her room's load beneath it, or the escape timer, message or exploded door
/// written over its characters afterwards.
/// </summary>
internal static class MotherBrainWholeFramesVerification
{
    /// <summary>Checks her dead brain's faces, her collapsed corpse and her body against their completing pages.</summary>
    internal static void VerifyMotherBrainWholeFrames()
    {
        IReadOnlyList<MotherBrainSpritePage> sequence = MotherBrainSpritePageDefinitions.Sequence;
        // Her legs are her first pages; her corpse's are the last of its sheet.
        int legsLoaded = sequence.ToList().FindIndex(page => page.Sheet != MotherBrainSpritePageSheet.Legs) - 1;
        int corpseLoaded = sequence.ToList().FindLastIndex(page => page.Sheet == MotherBrainSpritePageSheet.Corpse);
        AssertTrue(legsLoaded >= 0 && corpseLoaded > legsLoaded, "Mother Brain's pages load her legs first and her corpse later");
        int checkedFrames = 0;
        foreach (EnemyFrameId face in new[]
            {
                EnemyFrameId.MotherBrainHead6, EnemyFrameId.MotherBrainHead7, EnemyFrameId.MotherBrainHead8,
                EnemyFrameId.MotherBrainHead9, EnemyFrameId.MotherBrainHead10,
            })
            checkedFrames += AssertWhole(EnemyDefinitionId.MotherBrainHead, face, legsLoaded);
        foreach (EnemyFrameId corpse in new[] { EnemyFrameId.MotherBrainHead11, EnemyFrameId.MotherBrainHead12 })
            checkedFrames += AssertWhole(EnemyDefinitionId.MotherBrainHead, corpse, corpseLoaded);
        foreach (EnemyFrameId body in MotherBrainBodyVisualDefinitions.ShownFrames.Select(entry => entry.Value).Distinct())
            checkedFrames += AssertWhole(EnemyDefinitionId.MotherBrainBody, body, legsLoaded);
        Console.WriteLine($"  Mother Brain whole frames: {checkedFrames} frames each hold one picture, shown at every step they are drawn at.");
    }

    /// <summary>
    /// Requires <paramref name="frame"/> on <paramref name="enemy"/>'s sheet to hold only its picture of step
    /// <paramref name="complete"/>, and to show it at every step it is drawn at and, when drawn then, before the first.
    /// </summary>
    private static int AssertWhole(EnemyDefinitionId enemy, EnemyFrameId frame, int complete)
    {
        InstalledEnemySheets sheets = InstalledFixtures.EnemySheets;
        RoomEnemyGraphicsSetDefinition set = SetDrawing(sheets, enemy);
        string name = EnemySheetDefinitions.FrameName(frame);
        IReadOnlyList<FrameArtworkVariation> variations = sheets.Variations(set, enemy, name);
        if (variations.SingleOrDefault(variation => variation.Sequence == EncounterArtworkSequence.MotherBrainPages) is not { } pages)
        {
            // Her completing pages leave every character the frame reads as her room's load does, so its loaded picture is
            // its whole picture and the sheet holds that one.
            AssertEqual(0, variations.Count, $"{frame} follows no artwork sequence once its pages leave it as loaded");
            _ = sheets.For(set, enemy).Frame(name);
            return 1;
        }
        string whole = FrameArtworkVariation.PictureName(name, [new EncounterArtworkStep(EncounterArtworkSequence.MotherBrainPages, complete)]);
        string[] held = [.. FrameVariationPictures.Names(name, variations)];
        AssertTrue(held.SequenceEqual([whole]), $"{frame}'s sheet holds only its whole picture {whole}, not {string.Join(", ", held)}");
        // Loading the picture requires the sheet to hold it.
        _ = sheets.For(set, enemy).Frame(whole);
        for (int step = 0; step < pages.Pictures.Length; step++)
        {
            if (pages.DrawnAt(step))
                AssertEqual<int?>(complete, pages.PictureAt(step, name), $"{frame} drawn at page step {step} shows its whole picture");
        }
        if (pages.DrawnBeforeFirstStep)
            AssertEqual<int?>(complete, pages.PictureBeforeFirstStep, $"{frame} drawn before her first page shows its whole picture");
        return 1;
    }

    /// <summary>The first graphics set that installs a sheet of <paramref name="enemy"/>.</summary>
    private static RoomEnemyGraphicsSetDefinition SetDrawing(InstalledEnemySheets sheets, EnemyDefinitionId enemy)
    {
        string name = sheets.sheets.Keys.First(key => key.Enemy == enemy).Set;
        return RoomEnemyGraphicsSetLists.DrawnUnder.First(set => set.Name == name);
    }

    /// <summary>Runs the Mother Brain whole-frame check.</summary>
    [VerificationCommand("--mother-brain-whole-frames")]
    internal static int MotherBrainWholeFramesCommand()
    {
        Suite(nameof(VerifyMotherBrainWholeFrames), VerifyMotherBrainWholeFrames);
        return 0;
    }
}
