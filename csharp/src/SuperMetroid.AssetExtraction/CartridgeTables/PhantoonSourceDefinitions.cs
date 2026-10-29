
namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Phantoon tables extraction reads (#1413): the import pipeline's private knowledge of
/// the cartridge, decoded once into the installed Phantoon colour document.
/// </summary>
internal static class PhantoonSourceDefinitions
{
    /// <summary>$A7:CB41, the eight consecutive sixteen-colour health palettes $A7:DC0F selects from.</summary>
    internal const int HealthBandColors = 0xa7cb41;

    /// <summary>$A7:CA41, the sixteen-colour materialization fade-out target.</summary>
    internal const int FadeOutColors = 0xa7ca41;

    /// <summary>$A7:CA61, the Wrecked Ship power-on target $A7:DC5A interpolates after Phantoon's death.</summary>
    internal const int PowerOnColors = 0xa7ca61;

    /// <summary>
    /// The colours of the power-on target: the cartridge's background colours from the first through the seventh set in
    /// its order, which the installed document keeps in that order and the import decodes into the sets they target.
    /// </summary>
    internal const int PowerOnColourCount = 112;
}
