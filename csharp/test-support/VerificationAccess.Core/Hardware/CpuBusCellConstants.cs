using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="CpuBusCell"/> that only verification reads.</summary>
[CpuBusAccess]
internal static class CpuBusCellConstants
{
    /// <summary>A WRAM byte.</summary>
    public static CpuBusCell WramByte(string name, int address, string encoding = "u8", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, 1, encoding, aliasOf);
    /// <summary>A little-endian WRAM word.</summary>
    public static CpuBusCell WramWord(string name, int address, string encoding = "u16 little-endian", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, 2, encoding, aliasOf);
    /// <summary>A bounded WRAM byte range.</summary>
    public static CpuBusCell WramBytes(string name, int address, int length, string encoding = "u8[]", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, length, encoding, aliasOf);
    /// <summary>A bounded table of little-endian WRAM words.</summary>
    public static CpuBusCell WramWords(string name, int address, int count, string encoding = "u16[] little-endian", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, count * 2, encoding, aliasOf);
}
