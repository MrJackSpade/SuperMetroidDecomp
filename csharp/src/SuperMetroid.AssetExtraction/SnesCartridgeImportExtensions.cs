namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Import-only cartridge access for extractors that also pass a mutable address space.
/// </summary>
public static class SnesCartridgeImportExtensions
{
    /// <summary>Reads one cartridge byte through an address space's import-only capability.</summary>
    /// <param name="bus">Address space that must also expose an import cartridge source.</param>
    /// <param name="cpuAddress">SNES CPU address to read.</param>
    /// <returns>The cartridge byte mapped at <paramref name="cpuAddress"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not expose cartridge import access.</exception>
    public static byte ReadCartridgeByte(this ISnesAddressSpace bus, int cpuAddress) =>
        (bus as IImportCartridgeSource ?? throw new ArgumentException(
            "Cartridge extraction requires a cartridge import source.", nameof(bus)))
        .ReadCartridgeByte(cpuAddress);
}
