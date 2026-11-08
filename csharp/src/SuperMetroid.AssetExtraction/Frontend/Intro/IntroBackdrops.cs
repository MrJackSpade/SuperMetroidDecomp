using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The installed opening backdrops as the game reads them (#1544): colours the opening shows that no picture holds. The
/// game loads no palette document for them; every picture the opening draws is pre-coloured by the import.
/// </summary>
public sealed class IntroBackdrops
{
    /// <summary>Keeps both backdrops.</summary>
    /// <param name="opening">The opening narration's backdrop.</param>
    /// <param name="ceresFlight">The backdrop of the Ceres flight and the Ceres destruction.</param>
    internal IntroBackdrops(RgbColor opening, RgbColor ceresFlight)
    {
        Opening = opening;
        CeresFlight = ceresFlight;
    }

    /// <summary>The opening narration's backdrop.</summary>
    public RgbColor Opening { get; }

    /// <summary>The backdrop of the Ceres flight and the Ceres destruction, once they load the flight's colours.</summary>
    public RgbColor CeresFlight { get; }
}
