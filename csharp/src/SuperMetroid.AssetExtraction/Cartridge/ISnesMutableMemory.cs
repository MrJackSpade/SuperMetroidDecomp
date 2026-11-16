
namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// Read access to the console's mutable WRAM and battery-backed SRAM. Unlike
/// the legacy CPU-bus reader, this contract cannot return cartridge bytes.
/// </summary>
public interface ISnesMutableMemory
{
    /// <summary>Reads WRAM through bank $7E/$7F or a low-window system-bank mirror.</summary>
    byte ReadWorkRamByte(int cpuAddress);
}
