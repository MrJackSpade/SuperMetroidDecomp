using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rom;

/// <summary>
/// Makes a legacy native-data fallback's cartridge dependency explicit at its call site.
/// Installed asset paths do not request this source; import and reference diagnostics do.
/// </summary>
public static class CartridgeImportSource
{
    public static IImportCartridgeSource Require(ISnesAddressSpace bus) =>
        bus as IImportCartridgeSource ?? throw new ArgumentException(
            "This native-data fallback requires a cartridge import source.", nameof(bus));
}
