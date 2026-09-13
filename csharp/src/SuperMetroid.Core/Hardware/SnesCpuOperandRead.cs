namespace SuperMetroid.Core.Hardware;

/// <summary>Data reads for translated absolute-indexed CPU instructions with explicit operand-bus state.</summary>
public static class SnesCpuOperandRead
{
    /// <summary>
    /// Models a sixteen-bit LDA absolute,Y data access. The last fetched operand
    /// byte drives MDR before the low data byte; that byte then drives the high read.
    /// This is not a default value for missing devices: only decoded open-bus ranges
    /// accept it. Other unimplemented hardware continues through the strict bus.
    /// </summary>
    public static ushort ReadAbsoluteIndexedWord(ISnesAddressSpace bus, byte bank, ushort operand, ushort index)
    {
        int address = ((bank << 16) + operand + index) & SnesCpuAddressLayout.AddressMask;
        byte low = ReadData(bus, address, (byte)(operand >> 8));
        byte high = ReadData(bus, (address + 1) & SnesCpuAddressLayout.AddressMask, low);
        return (ushort)(low | high << 8);
    }

    private static byte ReadData(ISnesAddressSpace bus, int address, byte memoryDataRegister)
    {
        int bank = address >> 16, offset = address & 0xffff;
        // No SNES device drives the reserved end of the B-bus register window.
        // ROM/WRAM banks with the same low offset must not be treated as open bus.
        if ((bank & 0x40) == 0 && offset >= SnesCpuOpenBusWindows.ReservedBBusStart &&
            offset <= SnesCpuOpenBusWindows.ReservedBBusEnd)
            return memoryDataRegister;
        return bus.ReadByte(address);
    }
}

/// <summary>Proven undriven CPU address ranges, mirrored in banks $00-$3F/$80-$BF.</summary>
public static class SnesCpuOpenBusWindows
{
    /// <summary>$2184: first unused B-bus register after the WRAM port registers.</summary>
    public const int ReservedBBusStart = 0x2184;
    /// <summary>$21FF: final address in the mirrored B-bus register window.</summary>
    public const int ReservedBBusEnd = 0x21ff;
}
