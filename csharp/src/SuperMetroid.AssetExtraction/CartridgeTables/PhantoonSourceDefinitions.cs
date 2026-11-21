using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Phantoon tables extraction reads (#1413): the import pipeline's private knowledge of
/// the cartridge, decoded once into the installed Phantoon colour document.
/// </summary>
[EncodingOwner("cartridge addresses of Phantoon's colour tables")]
internal static class PhantoonSourceDefinitions
{
    /// <summary>$A7:CB41, the eight consecutive sixteen-colour health palettes $A7:DC0F selects from.</summary>
    internal const int HealthBandColors = 0xa7cb41;

    /// <summary>$A7:CA41, the sixteen-colour materialization fade-out target.</summary>
    internal const int FadeOutColors = 0xa7ca41;

    /// <summary>$A7:CA61, the Wrecked Ship power-on target $A7:DC5A interpolates after Phantoon's death.</summary>
    internal const int PowerOnColors = 0xa7ca61;

    /// <summary>The sixteen-colour health palette of <see cref="HealthBandColors"/> each health band shows, lowest band first.</summary>
    internal static IReadOnlyDictionary<PhantoonHealthBand, int> HealthBandPalettes { get; } = new Dictionary<PhantoonHealthBand, int>
    {
        [PhantoonHealthBandDefinitions.BelowEighth] = 0,
        [PhantoonHealthBandDefinitions.BelowQuarter] = 1,
        [PhantoonHealthBandDefinitions.BelowThreeEighths] = 2,
        [PhantoonHealthBandDefinitions.BelowHalf] = 3,
        [PhantoonHealthBandDefinitions.BelowFiveEighths] = 4,
        [PhantoonHealthBandDefinitions.BelowThreeQuarters] = 5,
        [PhantoonHealthBandDefinitions.BelowSevenEighths] = 6,
        [PhantoonHealthBandDefinitions.Unhurt] = 7,
    };

    /// <summary>
    /// The background colour group each part the power-on fades is painted in, which is also its sixteen colours' place in
    /// <see cref="PowerOnColors"/>.
    /// </summary>
    internal static IReadOnlyDictionary<WreckedShipPoweredPart, int> PowerOnGroups { get; } = new Dictionary<WreckedShipPoweredPart, int>
    {
        [WreckedShipPoweredPart.Shared] = 0,
        [WreckedShipPoweredPart.GreenDoors] = 1,
        [WreckedShipPoweredPart.MagentaDoors] = 2,
        [WreckedShipPoweredPart.BlueDoors] = 3,
        [WreckedShipPoweredPart.Foreground] = 4,
        [WreckedShipPoweredPart.Background] = 5,
        [WreckedShipPoweredPart.Lights] = 6,
    };
}
