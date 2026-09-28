namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Cartridge bytes for import and native-reference diagnostics. Gameplay
/// memory readers must not depend on this import-only contract.
/// </summary>
public interface IImportCartridgeSource
{
    /// <summary>Reads one populated upper-window LoROM byte.</summary>
    byte ReadCartridgeByte(int cpuAddress);
}
