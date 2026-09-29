namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Optional source for CPU register/expansion reads outside WRAM and SRAM.
/// The translated runtime does not implement this hardware; focused native-operand
/// fixtures may provide a device response. This is not an open-bus default.
/// </summary>
public interface ISnesCpuPeripheralSource
{
    byte ReadPeripheralByte(int cpuAddress);
}
