using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that the installed projectile beam files assemble into the beam catalog the loader builds.</summary>
internal static class ProjectileFileContractsVerification
{
    /// <summary>
    /// Compares the loader's beam catalog with one assembled from the installed beam PNGs, and checks the beam assembler
    /// owns its input and rejects an incomplete selection set.
    /// </summary>
    internal static void VerifyProjectileFileContracts(GameInstallation installed)
    {
        Ensure.NotNull(installed);
        // The beam pages are import data no installation holds (#1636): the cartridge-derived extraction's are assembled.
        string directory = CartridgeImportContent.ImportData(GameInstallationLayout.ProjectileDirectoryName);
        var expectedBeams = BeamTileCatalog.Load(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount).Select(BeamTileAtlasDefinitions.SelectionAt)
            .ToDictionary(BeamTileAtlasDefinitions.FileName, index => File.ReadAllBytes(
                Path.Combine(directory, BeamTileAtlasDefinitions.FileName(index)))));
        var compiledBeams = InstalledFixtures.ImportProjectiles.BeamTiles;
        AssertTrue(!File.Exists(Path.Combine(installed.ProjectileDirectory, BeamPaletteFile.FileName)),
            "the installation holds no beam colours: they are import data (#1637)");
        AssertTrue(ReferenceEquals(compiledBeams.Palettes, InstalledFixtures.ImportProjectiles.BeamPalettes),
            "the beam catalog and the import's read share the beam colours read from the import data");
        for (int selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
            AssertTrue(compiledBeams.Resolve(BeamTileCatalog.AssetFor(SamusBeamCombinationTableOrder.FromTableIndex(selection))).Span.SequenceEqual(
                expectedBeams.Resolve(BeamTileCatalog.AssetFor(SamusBeamCombinationTableOrder.FromTableIndex(selection))).Span),
                "file-context beam assembly retains every transfer byte and selection");
        var sheets = Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount).Select(BeamTileAtlasDefinitions.SelectionAt).Select(index =>
        {
            using var png = File.OpenRead(Path.Combine(directory, BeamTileAtlasDefinitions.FileName(index)));
            return BeamTileAtlas.Load(png, index);
        }).ToArray();
        var ownedBeams = BeamTileCatalog.FromAtlases(sheets, (compiledBeams.Palettes ?? throw new InvalidOperationException("Verification requires compiledBeams.Palettes.")), (compiledBeams.HyperBeamFxColors ?? throw new InvalidOperationException("Verification requires compiledBeams.HyperBeamFxColors.")));
        sheets[0] = sheets[1];
        AssertTrue(ownedBeams.Resolve(BeamTileCatalog.AssetFor(0)).Span.SequenceEqual(
            expectedBeams.Resolve(BeamTileCatalog.AssetFor(0)).Span), "beam assembler owns the input array");
        AssertThrows<ArgumentException>(() => BeamTileCatalog.FromAtlases([], compiledBeams.Palettes,
            compiledBeams.HyperBeamFxColors), "beam assembler rejects an incomplete selection set");
        Console.WriteLine("PASS projectile file contracts: the extracted beam pages assemble into the loaded beam catalog.");
    }

    /// <summary>Runs the projectile resource contract check against the current installation.</summary>
    [VerificationCommand("--projectile-file-contracts")]
    internal static int ProjectileFileContractsCommand()
    {
        Suite(nameof(VerifyProjectileFileContracts), () => VerifyProjectileFileContracts(RepositoryInstallation.Installation));
        return 0;
    }
}
