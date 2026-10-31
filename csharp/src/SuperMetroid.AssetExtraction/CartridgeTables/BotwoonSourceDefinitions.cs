using SuperMetroid.Core;


namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Botwoon data extraction reads (#1413): the import pipeline's private knowledge of the
/// cartridge, decoded once into the installed colour document.
/// </summary>
[EncodingOwner("cartridge addresses of Botwoon's colour tables")]
internal static class BotwoonSourceDefinitions
{
    /// <summary>$B3:971B-$981A, BotwoonHealthBasedPalettes: the eight sixteen-colour health images.</summary>
    internal const int HealthColors = 0xb3971b;
}
