using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction;

public static partial class GameAssetInstaller
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
        using FileStream gate = Lock(installation.Root);
        ValidateRequiredExtractedContent(installation);
        return installation;
    }

    /// <summary>Shared strict implementation; startup may use its exception to decide whether repair is needed.</summary>
    private static void ValidateRequiredExtractedContent(GameInstallation installation)
    {
        ValidateReceipt(installation);
        // This is the startup inventory, not a sample of whichever room was run.
        // The component table is the one ordered inventory of stock-resource checks;
        // completeness of each domain's required references is audited separately.
        foreach (InstallerComponent component in Components)
            component.Validate(installation.ContentDirectory);
    }

    /// <summary>The receipt names this format and source.</summary>
    private static void ValidateReceipt(GameInstallation installation)
    {
        string receiptPath = Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName);
        var receipt = JsonSerializer.Deserialize<InstallationReceipt>(File.ReadAllText(receiptPath));
        if (receipt is null || receipt.FormatVersion != GameInstallationLayout.FormatVersion)
            throw new InvalidDataException($"Installation receipt {receiptPath} requires format " +
                $"{GameInstallationLayout.FormatVersion}; found {receipt?.FormatVersion.ToString() ?? "null"}. Reimport the supported cartridge.");
        if (receipt.RomSha256 != SupportedCartridge.Sha256)
            throw new InvalidDataException($"Installation receipt {receiptPath} has unsupported source provenance.");
    }
}
