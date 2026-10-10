using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction;

/// <summary>Development-tool members of <see cref="GameAssetInstaller"/>; never linked by player hosts.</summary>
internal static class GameAssetInstallerTooling
{
    /// <summary>
    /// Strictly validates every required extracted resource using the exact startup
    /// contract. Unlike a repair-admission query, preserves the failing path/format
    /// exception for tools. Never opens a ROM or imports, repairs or replaces content.
    /// </summary>
    public static GameInstallation ValidateExtractedContent(string root)
    {
        var installation = new GameInstallation(Path.GetFullPath(root));
        if (!Directory.Exists(installation.ContentDirectory))
            throw new DirectoryNotFoundException($"Extracted content directory is missing: {installation.ContentDirectory}");
        GameAssetInstaller.ValidateRequiredExtractedContent(installation);
        return installation;
    }
}
