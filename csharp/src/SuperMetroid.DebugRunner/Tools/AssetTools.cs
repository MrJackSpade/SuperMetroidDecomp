using System.Security.Cryptography;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Developer tools that export cartridge source evidence and generate checked-in definition
/// catalogs. They run from the repository root against the repository ROM; their outputs go to
/// ignored <c>csharp/test-temp</c> folders or to the generated source files they own.
/// </summary>
internal static partial class AssetTools
{
    /// <summary>The repository's supported retail ROM, rejected when it is any other revision.</summary>
    internal static CartridgeImportAddressSpace LoadRepositoryRom()
    {
        string path = Path.GetFullPath("Super Metroid.smc");
        var rom = CartridgeImportAddressSpace.LoadRetailRom(path);
        string actual = Convert.ToHexString(SHA256.HashData(rom.Rom));
        if (!actual.Equals(SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"'{path}' is SHA-256 {actual}; the tools read the supported revision {SupportedCartridge.Sha256}.");
        return rom;
    }

    /// <summary>The repository ROM's extracted installation, shared with the verifier in ignored test-temp.</summary>
    private static readonly Lazy<GameInstallation> repositoryInstallation = new(() =>
    {
        string root = Path.GetFullPath("csharp/test-temp/verification-installed-content");
        return GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
    });

    private static readonly Lazy<Action<SuperMetroidRuntime>> repositoryRuntimeBindings =
        new(() => InstalledRuntimeBindings.Create(repositoryInstallation.Value));

    /// <summary>A gameplay runtime bound to the repository installation's presentation catalogs.</summary>
    private static SuperMetroidRuntime CreateInstalledRuntime(ISnesAddressSpace bus, bool playerInvincibilityEnabled = false)
    {
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled,
            initialPaletteArt: repositoryInstallation.Value.LoadGameplayBasePalettes());
        repositoryRuntimeBindings.Value(runtime);
        return runtime;
    }
}
