namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Explicit cartridge access for extractors whose existing API also passes an address
/// space to decompression/DMA helpers. It does not supply a generic CPU-bus read.
/// </summary>
public static class SnesCartridgeImportExtensions
{
    public static byte ReadCartridgeByte(this ISnesAddressSpace bus, int cpuAddress) =>
        (bus as IImportCartridgeSource ?? throw new ArgumentException(
            "Cartridge extraction requires a cartridge import source.", nameof(bus)))
        .ReadCartridgeByte(cpuAddress);
}
