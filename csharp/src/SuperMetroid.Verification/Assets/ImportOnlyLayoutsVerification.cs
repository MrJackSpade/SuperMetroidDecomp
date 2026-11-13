using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
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
        string intro = GameInstallationLayout.IntroCinematicDirectoryName;
        for (int page = 0; page < IntroCinematicArtworkFormat.BackgroundPageCount; page++)
            yield return (intro, IntroCinematicArtworkFormat.BackgroundPageFileName(page));
        yield return (intro, IntroCinematicArtworkFormat.PortraitTilemapFileName);
        yield return (intro, IntroCinematicArtworkFormat.InitialNarrationTilemapFileName);
        yield return (intro, IntroFinalLineTilemapFormat.FileName);
        yield return (intro, IntroEyeTilemapFormat.FileName);
        yield return (intro, ZebesRevealArtworkFormat.MapFileName);
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
