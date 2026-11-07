using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Confirms Mother Brain's sheets hold her frames whole (#1513, #1538): each frame her legs' or her corpse's look holds
/// has one finished picture under its own name, which no look gameplay shows changes, painting its frame over the
/// characters once that look has copied them; none pictures a look still to come, her room's load beneath it, or the
/// escape timer, message or exploded door written over its characters afterwards. A frame that look's copies never write
/// shows nothing.
/// </summary>
internal static class MotherBrainWholeFramesVerification
{
    /// <summary>Checks her dead brain's faces, her collapsed corpse and her body against the looks that hold them.</summary>
    internal static void VerifyMotherBrainWholeFrames()
    {
        AssertTrue(EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, EncounterLookUploads.MotherBrainLegsLook)
                < EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, EncounterLookUploads.MotherBrainCorpseLook),
            "Mother Brain's looks copy her legs first and her corpse later");
        int checkedFrames = 0;
        foreach (EnemyFrameId face in new[]
            {
                EnemyFrameId.MotherBrainHead6, EnemyFrameId.MotherBrainHead7, EnemyFrameId.MotherBrainHead8,
                EnemyFrameId.MotherBrainHead9, EnemyFrameId.MotherBrainHead10,
            })
            checkedFrames += AssertWhole(EnemyDefinitionId.MotherBrainHead, face, EncounterLookUploads.MotherBrainLegsLook);
        foreach (EnemyFrameId corpse in new[] { EnemyFrameId.MotherBrainHead11, EnemyFrameId.MotherBrainHead12 })
            checkedFrames += AssertWhole(EnemyDefinitionId.MotherBrainHead, corpse, EncounterLookUploads.MotherBrainCorpseLook);
        foreach (EnemyFrameId body in MotherBrainBodyVisualDefinitions.ShownFrames.Select(entry => entry.Value).Distinct())
            checkedFrames += AssertWhole(EnemyDefinitionId.MotherBrainBody, body, EncounterLookUploads.MotherBrainLegsLook);
        Console.WriteLine($"  Mother Brain whole frames: {checkedFrames} frames each hold one finished picture of the look that holds them.");
    }

    /// <summary>
    /// Requires <paramref name="frame"/> on <paramref name="enemy"/>'s sheet to hold one finished picture under its own name,
    /// painting its frame over the characters her <paramref name="look"/> look leaves, or to show nothing.
    /// </summary>
    private static int AssertWhole(EnemyDefinitionId enemy, EnemyFrameId frame, string look)
    {
        InstalledEnemySheets sheets = InstalledFixtures.EnemySheets;
        RoomEnemyGraphicsSetDefinition set = SetDrawing(sheets, enemy);
        string name = EnemySheetDefinitions.FrameName(frame);
        AssertEqual(0, sheets.ChangingEncounters(set, enemy, name).Count, $"no look gameplay shows changes {frame}");
        SpriteSheet sheet = sheets.For(set, enemy);
        // A frame her look's copies never write shows nothing: its characters hold another family's artwork (#1513).
        if (sheet.ShowsNothing(name))
            return 1;
        var origin = new Position16<ScreenPixels>(ScreenGeometry.ScreenWidthPixels / 2, ScreenGeometry.ScreenHeightPixels / 2);
        var draw = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy, 0), sheet.Frame(name), origin, SpriteMirror.None);
        UploadedPictureChecks.AssertSameInks([draw], [SheetFrameSources.CharacterDraw(draw,
                EnemySheetFixtures.LivePage(set, ComposedEncounter.MotherBrain, EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, look)))],
            $"{frame}'s finished picture in her {look} look", requirePixels: false);
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
