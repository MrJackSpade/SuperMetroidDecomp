using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rom;

/// <summary>
/// Import-only cartridge contract, unavailable to gameplay code.
/// </summary>
public static class CartridgeImportSource
{
    public static IImportCartridgeSource Require(ISnesAddressSpace bus) =>
        bus as IImportCartridgeSource ?? throw new ArgumentException(
            "This native-data fallback requires a cartridge import source.", nameof(bus));
}
