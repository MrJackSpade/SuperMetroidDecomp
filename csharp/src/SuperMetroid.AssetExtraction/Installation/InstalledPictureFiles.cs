namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Where an installation's pictures lie (#1636): every picture file under the content directory is a page of the picture
/// store (<see cref="PictureStoreFiles"/>), directly in its directory. A picture file anywhere else (a character or tile
/// page, an ink map, anything) is not an installed picture, and the installation holding it is rejected when its artwork
/// is loaded, naming every such file, so no page of the import's intermediate data passes for one.
/// </summary>
internal static class InstalledPictureFiles
{
    /// <summary>The extension of a picture file.</summary>
    private const string PictureExtension = ".png";

    /// <summary>Every file under a content directory, hidden and system files included, failing on one that cannot be read.</summary>
    private static readonly EnumerationOptions EveryFile = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = false,
        AttributesToSkip = 0,
        MatchCasing = MatchCasing.CaseInsensitive,
    };

    /// <summary>Requires every picture file under the content directory <paramref name="content"/> to be a page of its picture store.</summary>
    /// <exception cref="InvalidDataException">A picture file lies outside the picture store's directory; every such file is named.</exception>
    internal static void RequireOnlyStorePictures(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(content));
        string store = Path.Combine(root, PictureStoreFiles.DirectoryName);
        string[] stray = [.. Directory.EnumerateFiles(root, "*" + PictureExtension, EveryFile)
            .Where(path => string.Equals(Path.GetExtension(path), PictureExtension, StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(Path.GetDirectoryName(path), store, StringComparison.OrdinalIgnoreCase))
            .Select(path => Path.GetRelativePath(root, path))
            .Order(StringComparer.Ordinal)];
        if (stray.Length > 0)
            throw new InvalidDataException($"The installation's content {root} holds {stray.Length} picture file(s) outside its picture store {store}, "
                + $"none of them an installed picture: {string.Join(", ", stray)}.");
    }
}
