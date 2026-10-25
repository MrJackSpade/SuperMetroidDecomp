using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Kraid data extraction reads (#1413): the import pipeline's private knowledge of the
/// cartridge, decoded once into the installed colour documents and background artwork.
/// </summary>
[CpuBusBoundary("catalog: Kraid cartridge sources")]
internal static class KraidSourceDefinitions
{
    /// <summary>$A7:AAA6, Kraid initialization's sprite palette three: the sixteen initial rock target colours.</summary>
    internal const int InitialTargetColors = 0xa7aaa6;
}
