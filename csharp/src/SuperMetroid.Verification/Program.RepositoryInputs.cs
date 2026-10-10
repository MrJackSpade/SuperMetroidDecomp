internal static partial class Program
{
    /// <summary>
    /// The retail cartridge in the repository root. Verifiers read it, and the shared extracted
    /// installation (<see cref="RepositoryInstallation"/>), rather than taking input paths.
    /// </summary>
    private static string RepositoryRomPath => Path.GetFullPath("Super Metroid.smc");

    /// <summary>A fresh retail address space over <see cref="RepositoryRomPath"/>, owned by one suite.</summary>
    private static SuperMetroid.AssetExtraction.CartridgeImportAddressSpace LoadRepositoryRom() =>
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(RepositoryRomPath);
}
