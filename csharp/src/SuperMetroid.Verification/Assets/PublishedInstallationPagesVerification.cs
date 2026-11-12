using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks that a published installation holds only constructed, coloured pictures (#1636): the character and tile pages
/// are the import's own data, which the installer deletes with its import-data directory before it publishes, so the
/// installation holds no import-data directory and no page outside the picture store.
/// </summary>
internal static class PublishedInstallationPagesVerification
{
    /// <summary>
    /// Walks the shared repository installation's content: no directory is the import-data directory, and every
    /// <c>.png</c> file is a page of the picture store, directly under <c>game/pictures/</c>.
    /// </summary>
    internal static void VerifyPublishedInstallationPages()
    {
        string content = Path.GetFullPath(RepositoryInstallation.Installation.ContentDirectory);
        string[] importData = [.. Directory.EnumerateDirectories(content, GameInstallationLayout.ImportDataDirectoryName, SearchOption.AllDirectories)];
        AssertEqual(0, importData.Length,
            $"a published installation holds no {GameInstallationLayout.ImportDataDirectoryName} directory: {string.Join(", ", importData)}");
        string pictures = Path.Combine(content, PictureStoreFiles.DirectoryName);
        string[] pages = [.. Directory.EnumerateFiles(content, "*.png", SearchOption.AllDirectories)];
        string[] outside = [.. pages.Where(page => !string.Equals(Path.GetDirectoryName(page), pictures, StringComparison.OrdinalIgnoreCase))];
        AssertEqual(0, outside.Length,
            $"every installed picture page is in the picture store; outside it: {string.Join(", ", outside.Take(20))}");
        AssertTrue(pages.Length > 0, "the picture store holds the installation's pages");
        Console.WriteLine($"  Published installation: {pages.Length} picture-store pages and no import data or other page.");
    }

    /// <summary>Runs the published installation's page check over the shared repository installation.</summary>
    [VerificationCommand("--published-installation-pages")]
    internal static int PublishedInstallationPagesCommand()
    {
        Suite(nameof(VerifyPublishedInstallationPages), () => VerifyPublishedInstallationPages());
        return 0;
    }
}
