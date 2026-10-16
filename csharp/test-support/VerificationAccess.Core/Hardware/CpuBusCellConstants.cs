namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="CpuBusCell"/> that only verification reads.</summary>
[CpuBusAccess]
internal static class CpuBusCellConstants
{
    /// <summary>A WRAM byte.</summary>
    public static CpuBusCell WramByte(string name, int address, string encoding = "u8", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, 1, encoding, aliasOf);
    /// <summary>A bounded table of little-endian WRAM words.</summary>
    public static CpuBusCell WramWords(string name, int address, int count, string encoding = "u16[] little-endian", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, count * 2, encoding, aliasOf);
}
