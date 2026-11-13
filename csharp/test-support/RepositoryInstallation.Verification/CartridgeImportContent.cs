using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// The extraction components' output, extracted from the repository cartridge once per process into a scratch directory of
/// its own (#1636): each component's installed documents and the character and tile pages it keeps as import data, which a
/// published installation never holds. A check that needs characters, or composes pictures again, reads them from here,
/// the extractors called on the cartridge, never from the installed directory. The directory is deleted when the process
/// exits; <c>tools/verify-all.ps1</c> clears <c>scratch</c> of a killed run's leftovers.
/// </summary>
internal static class CartridgeImportContent
{
    /// <summary>The extracted content directory, extracted on first use.</summary>
    private static readonly Lazy<string> content = new(() => SharedFixtureTiming.Load("Fixture: cartridge import data", Extract));

    /// <summary>The content directory holding every extraction component's directory and the import-data directory.</summary>
    internal static string Directory => content.Value;

    /// <summary>The extracted directory of the component named <paramref name="name"/>, such as <see cref="GameInstallationLayout.MapDirectoryName"/>.</summary>
    internal static string Component(string name) => Path.Combine(Directory, name);

    /// <summary>The import-data directory of the component named <paramref name="name"/>, holding its pages.</summary>
    internal static string ImportData(string name) => ImportDataDirectory.Of(Component(name));

    /// <summary>
    /// A fresh read of the enemy, boss and area artwork as the import reads it, for a fixture that replaces part of it;
    /// checks that only read it share <see cref="InstalledFixtures.EnemyArtwork"/>.
    /// </summary>
    internal static SuperMetroid.AssetExtraction.Enemies.ImportEnemyArtwork FreshEnemyArtwork() =>
        SuperMetroid.AssetExtraction.Enemies.EnemyTileArtworkFiles.LoadForImport(ImportData(GameInstallationLayout.EnemyTileDirectoryName));

    /// <summary>Extracts every extraction component from the repository cartridge into a new scratch directory.</summary>
    [PrivateInstallation("The character and tile pages are import data no installation holds (#1636): checks that need characters read this cartridge-derived extraction, made once per process.")]
    private static string Extract()
    {
        var scratch = new TestScratchDirectory("cartridge-import");
        byte[] rom;
        using (Stream source = File.OpenRead(Path.GetFullPath("Super Metroid.smc")))
            rom = SupportedCartridge.Read(source);
        GameAssetInstallerConstants.ExtractImportSources(rom, scratch.Root);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => scratch.Dispose();
        return scratch.Root;
    }
}
