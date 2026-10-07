using System.Security.Cryptography;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
}
