namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Optional source for CPU register/expansion reads outside WRAM and SRAM.
/// The translated runtime does not implement this hardware; focused native-operand
/// fixtures may provide a device response. This is not an open-bus default.
/// </summary>
public interface ISnesCpuPeripheralSource
{
    /// <summary>Supplies one explicit device response for a translated CPU operand read outside supported WRAM/SRAM and compiled cartridge definitions; must not silently substitute an open-bus default for unimplemented hardware.</summary>
    /// <param name="cpuAddress">Full 24-bit CPU bus address, including its bank, already formed and wrapped by the instruction-specific reader.</param>
    /// <returns>The byte driven by the modeled register or peripheral; instruction-specific undriven ranges are handled by the caller before this request.</returns>
    byte ReadPeripheralByte(int cpuAddress);
}
