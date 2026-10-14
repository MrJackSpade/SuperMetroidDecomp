namespace SuperMetroid.Verification.Harness;

/// <summary>Gives verifier suites the repository's pinned retail cartridge input.</summary>
internal static class RepositoryInputsVerification
{
    /// <summary>
    /// The retail cartridge in the repository root. Verifiers read it, and the shared extracted
    /// installation (<see cref="RepositoryInstallation"/>), rather than taking input paths.
    /// </summary>
    internal static string RepositoryRomPath => Path.GetFullPath("Super Metroid.smc");

    /// <summary>A fresh retail address space over <see cref="RepositoryRomPath"/>, owned by one suite.</summary>
    internal static SuperMetroid.AssetExtraction.CartridgeImportAddressSpace LoadRepositoryRom() =>
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(RepositoryRomPath);
}
