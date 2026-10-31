namespace SuperMetroid.Core.Map;

/// <summary>Cartridge places of the legacy save image's explored-map packing tables that only verification reads.</summary>
internal static class ExploredMapPackingDefinitionsConstants
{
    /// <summary>$81:8131, six live area byte counts followed by unused Ceres.</summary>
    public const int NativeByteCountTable = 0x818131;
    /// <summary>$81:8138, native packed SRAM destination offsets.</summary>
    public const int NativeDestinationOffsetTable = 0x818138;
    /// <summary>$81:82D6, pointers to native sparse byte-index lists.</summary>
    public const int NativeSourcePointerTable = 0x8182d6;
}
