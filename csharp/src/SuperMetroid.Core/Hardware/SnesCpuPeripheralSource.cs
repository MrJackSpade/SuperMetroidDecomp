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

/// <summary>
/// Classifies a dynamically computed CPU data address before dispatching to a typed
/// source. Only instruction emulation and corruption-derived pointers use this path;
/// immutable definitions must be installed before runtime.
/// </summary>
internal static class SnesCpuMappedData
{
    internal static byte ReadByte(ISnesAddressSpace bus, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam =>
                (bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                    "CPU data read requires WRAM.")).ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam =>
                (bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                    "CPU data read requires SRAM.")).ReadSaveRamByte(address),
            SnesDmaSourceKind.Cartridge => throw new InvalidOperationException(
                $"CPU read ${address >> 16:X2}:{address & 0xffff:X4} requests cartridge data " +
                "that has not been installed as a compiled definition."),
            _ => ReadPeripheral(bus, address),
        };

    private static byte ReadPeripheral(ISnesAddressSpace bus, int address)
    {
        if (bus is ISnesCpuPeripheralSource peripheral)
            return peripheral.ReadPeripheralByte(address);
        throw new InvalidOperationException(
            $"CPU read ${address >> 16:X2}:{address & 0xffff:X4} is outside the runtime address map.");
    }
}
