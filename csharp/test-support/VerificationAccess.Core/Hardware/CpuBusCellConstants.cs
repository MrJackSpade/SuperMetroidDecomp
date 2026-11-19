using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="CpuBusCell"/> that only verification reads.</summary>
internal static class CpuBusCellConstants
{
    /// <summary>A WRAM byte.</summary>
    public static CpuBusCell WramByte(string name, int address, string encoding = "u8", CpuBusCell? aliasOf = null) =>
        global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell>([typeof(string), typeof(global::SuperMetroid.AssetExtraction.Cartridge.CpuBusRegion), typeof(int), typeof(int), typeof(string), typeof(global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell)], new object?[] { (string)(name), (global::SuperMetroid.AssetExtraction.Cartridge.CpuBusRegion)(CpuBusRegion.WorkRam), (int)(address), (int)(1), (string)(encoding), (global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell?)(aliasOf) });
    /// <summary>A little-endian WRAM word.</summary>
    public static CpuBusCell WramWord(string name, int address, string encoding = "u16 little-endian", CpuBusCell? aliasOf = null) =>
        global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell>([typeof(string), typeof(global::SuperMetroid.AssetExtraction.Cartridge.CpuBusRegion), typeof(int), typeof(int), typeof(string), typeof(global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell)], new object?[] { (string)(name), (global::SuperMetroid.AssetExtraction.Cartridge.CpuBusRegion)(CpuBusRegion.WorkRam), (int)(address), (int)(2), (string)(encoding), (global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell?)(aliasOf) });
    /// <summary>A bounded WRAM byte range.</summary>
    public static CpuBusCell WramBytes(string name, int address, int length, string encoding = "u8[]", CpuBusCell? aliasOf = null) =>
        global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell>([typeof(string), typeof(global::SuperMetroid.AssetExtraction.Cartridge.CpuBusRegion), typeof(int), typeof(int), typeof(string), typeof(global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell)], new object?[] { (string)(name), (global::SuperMetroid.AssetExtraction.Cartridge.CpuBusRegion)(CpuBusRegion.WorkRam), (int)(address), (int)(length), (string)(encoding), (global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell?)(aliasOf) });
    /// <summary>A bounded table of little-endian WRAM words.</summary>
    public static CpuBusCell WramWords(string name, int address, int count, string encoding = "u16[] little-endian", CpuBusCell? aliasOf = null) =>
        global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell>([typeof(string), typeof(global::SuperMetroid.AssetExtraction.Cartridge.CpuBusRegion), typeof(int), typeof(int), typeof(string), typeof(global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell)], new object?[] { (string)(name), (global::SuperMetroid.AssetExtraction.Cartridge.CpuBusRegion)(CpuBusRegion.WorkRam), (int)(address), (int)(count * 2), (string)(encoding), (global::SuperMetroid.AssetExtraction.Cartridge.CpuBusCell?)(aliasOf) });
}
