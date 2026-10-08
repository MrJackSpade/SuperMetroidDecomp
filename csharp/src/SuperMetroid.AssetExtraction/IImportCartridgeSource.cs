namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Cartridge bytes available only to the asset-import assembly.
/// </summary>
public interface IImportCartridgeSource
{
    /// <summary>Reads a cartridge byte by its CPU-visible address for asset extraction.</summary>
    /// <param name="cpuAddress">The 24-bit SNES address of the cartridge data, rather than a file offset.</param>
    /// <returns>The encoded byte at the mapped cartridge location.</returns>
    byte ReadCartridgeByte(int cpuAddress);
}
