using System.Security.Cryptography;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.DebugRunner.Tools;

/// <summary>The repository ROM the developer tools read.</summary>
internal static class RepositoryRom
{
    /// <summary>The repository's supported retail ROM, rejected when it is any other revision.</summary>
    internal static CartridgeImportAddressSpace Load()
    {
        string path = Path.GetFullPath("Super Metroid.smc");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(path);
        string actual = Convert.ToHexString(SHA256.HashData(rom.Rom));
        if (!actual.Equals(SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"'{path}' is SHA-256 {actual}; the tools read the supported revision {SupportedCartridge.Sha256}.");
        return rom;
    }
}
