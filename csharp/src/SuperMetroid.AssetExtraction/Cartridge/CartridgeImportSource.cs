
namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// Import-only cartridge contract, unavailable to gameplay code.
/// </summary>
public static class CartridgeImportSource
{
    /// <summary>
    /// The address space an import boundary is handed, viewed as the cartridge source every address space is (#2554), so
    /// extraction code names the cartridge as the owner of the bytes it reads.
    /// </summary>
    /// <param name="bus">Address space supplied to an import boundary.</param>
    /// <returns>The same object viewed as an import cartridge source.</returns>
    public static IImportCartridgeSource Require(ISnesAddressSpace bus) => Ensure.NotNull(bus);
}
