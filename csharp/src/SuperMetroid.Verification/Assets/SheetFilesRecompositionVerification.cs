using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms the sheet composers and the picture store reproduce the installation's sheet files byte for byte (#1392,
/// #1635). The picture store holds every sheet family's pictures, each once, and the page and corner each sheet manifest
/// names for a frame's picture depend on every family's pictures before it, so the four families are composed again
/// together, as the installer composes them, and the store is written over all of them. Each family's directory then holds
/// its sheet manifests, its provenance and its other installed files, and the store's directory, with the recolour-region
/// and blend-map directories, is compared as a whole: no sheet picture or per-sheet map is installed outside the store.
/// </summary>
internal static class SheetFilesRecompositionVerification
{
    /// <summary>
    /// The installed directories the sheet families and the picture store write, relative to the content directory: each
    /// family's, the store's, and the maps' directories, which hold only the store's maps.
    /// </summary>
    private static readonly string[] WrittenDirectories =
    [
        GameInstallationLayout.SpriteSheetDirectoryName,
        GameInstallationLayout.RoomBlockSheetDirectoryName,
        GameInstallationLayout.BossBodySheetDirectoryName,
        GameInstallationLayout.EnemySheetDirectoryName,
        PictureStoreFiles.DirectoryName,
        GameInstallationLayout.RecolourRegionDirectoryName,
        GameInstallationLayout.BlendMapDirectoryName,
    ];

    /// <summary>
    /// Composes every sheet family and writes the picture store into a scratch content directory, then compares every
    /// directory they write with the installed one, file by file and byte for byte.
    /// </summary>
    [PrivateInstallation("Checks the sheet composers and the picture store writer themselves: composing every sheet family again from the installed sources and writing the picture store over them must reproduce the installed sheet manifests, the store's pages and maps and their provenance byte for byte, which only a fresh composition can show.")]
    internal static void VerifySheetFilesRecompose(ISnesAddressSpace bus)
    {
        DirectoryInfo scratch = Directory.CreateTempSubdirectory("sm-sheet-files-");
        try
        {
            string installed = RepositoryInstallation.Installation.ContentDirectory;
            string recomposed = Path.Combine(scratch.FullName, GameInstallationLayout.ContentDirectoryName);
            // The sheets compose from the stock sources installed in the shared installation's content directory, and are
            // written into the scratch one; the store takes the families in its own fixed order, whatever order they compose in.
            var families = new InstalledSheetFamilies();
            SpriteSheetFiles.Extract(bus, installed, Path.Combine(recomposed, GameInstallationLayout.SpriteSheetDirectoryName), families);
            RoomBlockSheetFiles.Extract(installed, Path.Combine(recomposed, GameInstallationLayout.RoomBlockSheetDirectoryName), families);
            BossBodySheetFiles.Extract(installed, Path.Combine(recomposed, GameInstallationLayout.BossBodySheetDirectoryName), families);
            EnemySheetFiles.Extract(installed, Path.Combine(recomposed, GameInstallationLayout.EnemySheetDirectoryName), families);
            PictureStoreFiles.Write(recomposed, families.Every(), SupportedCartridge.Sha256);
            int files = 0;
            foreach (string directory in WrittenDirectories)
                files += AssertSameFiles(Path.Combine(installed, directory), Path.Combine(recomposed, directory), directory);
            Console.WriteLine($"Sheet files: {files} files of the sheet families and the picture store recompose byte for byte.");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    /// <summary>
    /// Fails unless <paramref name="recomposed"/> holds exactly the files of <paramref name="installed"/>, at the same
    /// relative paths and with the same bytes, or neither directory exists.
    /// </summary>
    /// <returns>How many files were compared.</returns>
    private static int AssertSameFiles(string installed, string recomposed, string label)
    {
        AssertEqual(Directory.Exists(installed), Directory.Exists(recomposed), $"the recomposition writes directory {label} exactly when the installation holds it");
        if (!Directory.Exists(installed))
            return 0;
        string[] expected = RelativeFiles(installed);
        string[] actual = RelativeFiles(recomposed);
        AssertTrue(expected.SequenceEqual(actual, StringComparer.Ordinal),
            $"the recomposed files of {label} are the installed files: only installed {string.Join(", ", expected.Except(actual, StringComparer.Ordinal))}; "
            + $"only recomposed {string.Join(", ", actual.Except(expected, StringComparer.Ordinal))}");
        foreach (string file in expected)
        {
            AssertTrue(File.ReadAllBytes(Path.Combine(installed, file)).AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(recomposed, file))),
                $"{label} file {file} recomposes byte for byte");
        }
        return expected.Length;
    }

    /// <summary>Every file under <paramref name="directory"/>, by its path relative to it, in ordinal order.</summary>
    private static string[] RelativeFiles(string directory) =>
        [.. Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(directory, file)).Order(StringComparer.Ordinal)];

    /// <summary>Runs the sheet files' recomposition check against the repository cartridge.</summary>
    [VerificationCommand("--sheet-files-recompose")]
    internal static int SheetFilesRecomposeCommand()
    {
        Suite(nameof(VerifySheetFilesRecompose), () => VerifySheetFilesRecompose(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath(RepositoryRomPath))));
        return 0;
    }
}
