namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One CPU data-bus byte at a computed address that a native instruction dereferences
/// (#629): a corrupted pointer, a table over-read, or an indexed operand. WRAM and SRAM come
/// from live memory and other non-cartridge windows from a modeled peripheral; cartridge
/// windows are left to the caller's compiled definition. Only domain memory views use it.
/// </summary>
[CpuBusAccess]
internal static class SnesLiveDataRead
{
    /// <summary>
    /// Reads the live byte at <paramref name="address"/>, or returns <see langword="false"/>
    /// when the address selects a cartridge window. <paramref name="reader"/> names the native
    /// access in failures for missing live memory or an unimplemented peripheral.
    /// </summary>
    internal static bool TryRead(ISnesAddressSpace bus, int address, string reader, out byte value)
    {
        switch (SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)))
        {
            case SnesDmaSourceKind.WorkRam:
                value = (bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                    $"{reader} requires WRAM.")).ReadWorkRamByte(address);
                return true;
            case SnesDmaSourceKind.SaveRam:
                value = (bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                    $"{reader} requires SRAM.")).ReadSaveRamByte(address);
                return true;
            case SnesDmaSourceKind.Unmapped:
                value = (bus as ISnesCpuPeripheralSource ?? throw new InvalidOperationException(
                    $"{reader} ${address:X6} requires an unimplemented peripheral.")).ReadPeripheralByte(address);
                return true;
            case SnesDmaSourceKind.Cartridge:
                value = 0;
                return false;
            default:
                throw new InvalidOperationException($"Undefined CPU source kind for {reader} ${address:X6}.");
        }
    }
}
