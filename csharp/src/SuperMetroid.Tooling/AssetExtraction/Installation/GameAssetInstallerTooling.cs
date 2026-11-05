namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Development-tool members of <see cref="GameAssetInstaller"/>; never linked by player hosts.</summary>
internal static class GameAssetInstallerTooling
{
    /// <summary>
    /// The strict form of the startup decision, for tools: the content's receipt must name this importer's format and the
    /// supported source, or the exception says why it does not. Published content never changes, so its files are not
    /// read; a damaged one fails loudly where it is loaded. Never opens a ROM or imports, repairs or replaces content.
    /// </summary>
    public static GameInstallation ValidateExtractedContent(string root)
    {
        var installation = new GameInstallation(Path.GetFullPath(root));
        if (!Directory.Exists(installation.ContentDirectory))
            throw new DirectoryNotFoundException($"Extracted content directory is missing: {installation.ContentDirectory}");
        string receiptPath = Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName);
        if (GameAssetInstaller.ReceiptMismatchAt(receiptPath) is { } mismatch)
            throw new InvalidDataException(mismatch);
        return installation;
    }
}
