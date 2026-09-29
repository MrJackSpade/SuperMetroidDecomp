namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Cartridge bytes available only to the asset-import assembly.
/// </summary>
public interface IImportCartridgeSource
{
    byte ReadCartridgeByte(int cpuAddress);
}
