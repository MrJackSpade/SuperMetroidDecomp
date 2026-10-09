using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rom;

/// <summary>
/// Import-only cartridge contract, unavailable to gameplay code.
/// </summary>
public static class CartridgeImportSource
{
    /// <summary>Requires an address space that exposes the import-only cartridge capability used by extraction code.</summary>
    /// <param name="bus">Address space supplied to an import boundary.</param>
    /// <returns>The same object viewed as an import cartridge source.</returns>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not expose cartridge bytes, including when it is null.</exception>
    public static IImportCartridgeSource Require(ISnesAddressSpace bus) =>
        bus as IImportCartridgeSource ?? throw new ArgumentException(
            "This native-data fallback requires a cartridge import source.", nameof(bus));
}
