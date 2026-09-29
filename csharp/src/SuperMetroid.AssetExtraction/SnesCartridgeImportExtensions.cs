namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Import-only cartridge access for extractors that also pass a mutable address space.
/// </summary>
public static class SnesCartridgeImportExtensions
{
    public static byte ReadCartridgeByte(this ISnesAddressSpace bus, int cpuAddress) =>
        (bus as IImportCartridgeSource ?? throw new ArgumentException(
            "Cartridge extraction requires a cartridge import source.", nameof(bus)))
        .ReadCartridgeByte(cpuAddress);
}
