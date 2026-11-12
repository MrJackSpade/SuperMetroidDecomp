using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Cartridge values and helpers of <see cref="GameAssetInstaller"/> that only verification reads.</summary>
internal static class GameAssetInstallerConstants
{
    /// <summary>
    /// Extracts every extraction component from <paramref name="rom"/> into <paramref name="content"/>, all at once, with
    /// their character and tile pages under its import-data directory (#1636), and nothing else: no audio, no sheets and no
    /// picture store. Checks that compose pictures again read their characters from this cartridge-derived extraction,
    /// never from a published installation, which holds none.
    /// </summary>
    /// <param name="rom">The validated cartridge image.</param>
    /// <param name="content">The empty directory the components' directories are extracted into.</param>
    /// <exception cref="AggregateException">A component failed; each failure is one of its inner exceptions.</exception>
    internal static void ExtractImportSources(byte[] rom, string content)
    {
        Ensure.NotNull(rom);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        Directory.CreateDirectory(content);
        var families = new InstalledSheetFamilies();
        Parallel.ForEach(GameAssetInstaller.Components.Where(component => component.Kind == InstallerComponentKind.Extraction),
            component => GameAssetInstaller.InstallComponent(rom, content, component, families, CancellationToken.None, progress: null));
    }
}
