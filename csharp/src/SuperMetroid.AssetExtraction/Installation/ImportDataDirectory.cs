namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Where the import keeps a component's character and tile pages while it installs (#1636): beside the component's own
/// directory, under the staging directory's <see cref="GameInstallationLayout.ImportDataDirectoryName"/> child, which the
/// installer deletes before it publishes. Every extraction component that decodes pages writes them here, and every
/// composition component reads them from here; nothing the game loads reads it.
/// </summary>
public static class ImportDataDirectory
{
    /// <summary>
    /// The import-data directory of the component whose directory is <paramref name="componentDirectory"/>:
    /// <c>&lt;content&gt;/import-data/&lt;component&gt;</c> for <c>&lt;content&gt;/&lt;component&gt;</c>.
    /// </summary>
    /// <param name="componentDirectory">The component's directory under the staging (or extracted content) directory.</param>
    /// <exception cref="ArgumentException">The directory is empty or has no content directory above it.</exception>
    public static string Of(string componentDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(componentDirectory);
        string component = Path.TrimEndingDirectorySeparator(Path.GetFullPath(componentDirectory));
        string content = Path.GetDirectoryName(component)
            ?? throw new ArgumentException($"Component directory {componentDirectory} has no content directory above it.", nameof(componentDirectory));
        return Path.Combine(content, GameInstallationLayout.ImportDataDirectoryName, Path.GetFileName(component));
    }
}
