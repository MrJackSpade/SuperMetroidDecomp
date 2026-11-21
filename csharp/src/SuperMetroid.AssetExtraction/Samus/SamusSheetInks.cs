namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Where Samus's colourings lie among her sheets' inks (#1512): the import composes her sheets with her suit in the
/// fifth sprite colour group, her grapple flare as the sixth group's last colour, her beams in the seventh and her body
/// without the suit in the eighth, as the cartridge's sprite palettes place them. An ink is its group's place times
/// sixteen plus its colour. Inks are the composed pixels' indices only the import sees: the installation keys each pixel by
/// the colour it shows (<see cref="SuperMetroid.AssetExtraction.Artwork.InstalledSheetConversion"/>), so nothing installed
/// names an ink.
/// </summary>
internal static class SamusSheetInks
{
    /// <summary>The colours of one sprite colour group.</summary>
    internal const int GroupColours = 16;

    /// <summary>The first ink of her suit's colours.</summary>
    internal const int Suit = 4 * GroupColours;

    /// <summary>The ink of her grapple flare's colour.</summary>
    internal const int GrappleFlare = 5 * GroupColours + 15;

    /// <summary>The first ink of her beams' colours.</summary>
    internal const int Beams = 6 * GroupColours;

    /// <summary>The first ink of her body's colours without the suit.</summary>
    internal const int Suitless = 7 * GroupColours;

    /// <summary>The ink of her visor's colour, among her suit's.</summary>
    internal const int Visor = Suit + 4;

    /// <summary>The first ink of the Crystal Flash bubble's colours, among her beams'.</summary>
    internal const int CrystalFlashBubble = Beams + 10;

    /// <summary>The first ink of the Hyper Beam cycle's colours, among her beams'.</summary>
    internal const int HyperBeamCycle = Beams + 1;
}
