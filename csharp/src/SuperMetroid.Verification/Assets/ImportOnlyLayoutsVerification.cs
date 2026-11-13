using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks that the layouts whose entries are the original's packed tile placements, and which only the import composes
/// pictures from, are import data (#1638): the extraction writes each to its component's import data, and a published
/// installation, which holds the pictures they compose, holds none of them.
/// </summary>
internal static class ImportOnlyLayoutsVerification
{
    /// <summary>Each import-only layout, by the component directory it belongs to and its file name.</summary>
    private static IEnumerable<(string Component, string File)> Layouts()
    {
        string enemyTiles = GameInstallationLayout.EnemyTileDirectoryName;
        yield return (enemyTiles, CrocomireMeltingArtworkFormat.FirstTilemapFileName);
        yield return (enemyTiles, CrocomireMeltingArtworkFormat.SecondTilemapFileName);
        yield return (enemyTiles, KraidBackgroundArtworkFormat.UpperFileName);
        yield return (enemyTiles, KraidBackgroundArtworkFormat.LowerFileName);
        foreach (KraidHeadFrame head in KraidHeadTilemapFormat.HeadsByMouthStage)
            yield return (enemyTiles, KraidBackgroundArtworkFormat.HeadFileName(head.MouthStage));
        yield return (enemyTiles, CeresWarningGlyphDefinitions.FileName);
        yield return (enemyTiles, PhantoonBg2FrameDefinitions.FileName);
        yield return (enemyTiles, DraygonBg2FrameDefinitions.FileName);
        yield return (enemyTiles, CrocomireBg2FrameDefinitions.FileName);
        yield return (enemyTiles, MotherBrainBodyBg2FrameDefinitions.FileName);
        yield return (enemyTiles, EnemyProjectileSpritemapDefinitions.FileName);
        yield return (enemyTiles, SuperMetroid.AssetExtraction.World.Artwork.EnemySpritemapDefinitions.FileName);
        yield return (enemyTiles, SuperMetroid.AssetExtraction.World.Artwork.EnemyExtendedFrameDefinitions.FileName);
        string intro = GameInstallationLayout.IntroCinematicDirectoryName;
        for (int page = 0; page < IntroCinematicArtworkFormat.BackgroundPageCount; page++)
            yield return (intro, IntroCinematicArtworkFormat.BackgroundPageFileName(page));
        yield return (intro, IntroCinematicArtworkFormat.PortraitTilemapFileName);
        yield return (intro, IntroCinematicArtworkFormat.InitialNarrationTilemapFileName);
        yield return (intro, IntroFinalLineTilemapFormat.FileName);
        yield return (intro, IntroEyeTilemapFormat.FileName);
        yield return (intro, ZebesRevealArtworkFormat.MapFileName);
        yield return (intro, IntroCaretSpriteFormat.FileName);
        yield return (intro, IntroNextPageArrowSpriteFormat.FileName);
        yield return (intro, IntroMotherBrainSpriteFormat.FileName);
        yield return (intro, IntroMotherBrainExplosionSpriteFormat.FileName);
        yield return (intro, IntroRinkaSpriteFormat.FileName);
        yield return (intro, IntroEggEffectSpriteFormat.FileName);
        yield return (intro, IntroDiscoveryActorSpriteFormat.FileName);
        yield return (intro, IntroScientistSpriteFormat.FileName);
        yield return (intro, CeresFlightSpriteFormat.FileName);
        yield return (intro, CeresDestructionSpriteFormat.FileName);
        foreach (string document in MapPresentationFiles.ImportDocumentFiles)
            yield return (GameInstallationLayout.MapDirectoryName, document);
        string ending = GameInstallationLayout.EndingObjectDirectoryName;
        for (int fragment = 0; fragment < SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectArtworkFormat.FragmentCount; fragment++)
            yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectArtworkFormat.FragmentFileName(fragment));
        yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectArtworkFormat.WaitingTilemapFileName);
        yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectArtworkFormat.PostCreditsFragmentBFileName);
        yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectArtworkFormat.PostShotLogoMapFileName);
        yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingCloudSpriteFormat.FileName);
        yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingExplosionSpriteFormat.FileName);
        yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingCompletionTextSpriteFormat.FileName);
        yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingRewardSpriteFormat.FileName);
        yield return (ending, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingLogoSpriteFormat.FileName);
        // The ending's Mode 7 cell maps and their manifest only compose the floors' sheets (#1693).
        string endingMode7 = GameInstallationLayout.EndingMode7DirectoryName;
        foreach (SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingFloorSceneId floor in Enum.GetValues<SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingFloorSceneId>())
            yield return (endingMode7, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingFloorArtworkFormat.MapFileName(floor));
        yield return (endingMode7, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingRewardIconArtworkFormat.MapFileName);
        yield return (endingMode7, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingFloorArtworkFormat.ManifestFileName);
        string projectiles = GameInstallationLayout.ProjectileDirectoryName;
        yield return (projectiles, ProjectileSpriteDefinitions.FileName);
        yield return (projectiles, ChargeFlareSpriteExtractor.FileName);
        yield return (projectiles, ProjectileTrailVisualDefinitions.FileName);
        yield return (projectiles, GrappleSpriteDefinitions.FileName);
        string samusBody = GameInstallationLayout.SamusBodyDirectoryName;
        yield return (samusBody, SamusBodyArtworkFiles.ArtworkFileName);
        yield return (samusBody, SamusAtmosphericArtworkFiles.ArtworkFileName);
        yield return (samusBody, SamusAtmosphericArtworkFiles.ManifestFileName);
        yield return (samusBody, SamusArmCannonArtworkFormat.ArtworkFileName);
    }

    /// <summary>
    /// Every import-only layout is in the cartridge-derived extraction's import data and absent from the shared repository
    /// installation's component directory.
    /// </summary>
    internal static void VerifyImportOnlyLayouts()
    {
        string content = Path.GetFullPath(RepositoryInstallation.Installation.ContentDirectory);
        int count = 0;
        foreach ((string component, string file) in Layouts())
        {
            AssertTrue(File.Exists(Path.Combine(CartridgeImportContent.ImportData(component), file)),
                $"the extraction keeps {component}/{file} as import data");
            AssertTrue(!File.Exists(Path.Combine(content, component, file)),
                $"a published installation does not hold the import-only layout {component}/{file}");
            count++;
        }
        Console.WriteLine($"  Import-only layouts: {count} layouts are import data and none is installed.");
    }

    /// <summary>Runs the import-only layout check over the shared repository installation.</summary>
    [VerificationCommand("--import-only-layouts")]
    internal static int ImportOnlyLayoutsCommand()
    {
        Suite(nameof(VerifyImportOnlyLayouts), () => VerifyImportOnlyLayouts());
        return 0;
    }
}
