
namespace SuperMetroid.Core.Map;

/// <summary>Cartridge values of <see cref="ExploredMapPackingDefinitions"/> that only verification reads.</summary>
internal static class ExploredMapPackingDefinitionsConstants
{
    /// <summary>$81:8131, six live area byte counts followed by unused Ceres.</summary>
    public const int NativeByteCountTable = 0x818131;
    /// <summary>$81:8138, native packed SRAM destination offsets.</summary>
    public const int NativeDestinationOffsetTable = 0x818138;
    /// <summary>$81:82D6, pointers to native sparse byte-index lists.</summary>
    public const int NativeSourcePointerTable = 0x8182d6;

    /// <summary>Provides native explored-map byte counts, destinations, and source pointers for map packing checks.</summary>
    extension(ExploredMapPackingDefinitions)
    {
        /// <inheritdoc cref="ExploredMapPackingDefinitionsConstants.NativeByteCountTable"/>
        internal static int NativeByteCountTable => ExploredMapPackingDefinitionsConstants.NativeByteCountTable;
        /// <inheritdoc cref="ExploredMapPackingDefinitionsConstants.NativeDestinationOffsetTable"/>
        internal static int NativeDestinationOffsetTable => ExploredMapPackingDefinitionsConstants.NativeDestinationOffsetTable;
        /// <inheritdoc cref="ExploredMapPackingDefinitionsConstants.NativeSourcePointerTable"/>
        internal static int NativeSourcePointerTable => ExploredMapPackingDefinitionsConstants.NativeSourcePointerTable;
    }
}
