namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of Ceres Ridley's health bands (#2242). His fight counts accepted hits in place of health: he is in the highest
/// band whose <see cref="MinimumHits"/> his count reaches, and in <see cref="CeresRidleyHealthBandDefinitions.Unhurt"/>
/// below them all.
/// </summary>
public sealed partial class CeresRidleyHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="minimumHits">The fewest accepted hits in the band: none for the band he starts in.</param>
    /// <param name="healthShade">The health colouring laid over his own colours in the band, or null where none is laid.</param>
    internal CeresRidleyHealthBand(int minimumHits, RidleyHealthShade? healthShade)
    {
        MinimumHits = minimumHits;
        HealthShade = healthShade;
    }

    /// <summary>The fewest accepted hits in the band: none for the band he starts in.</summary>
    public int MinimumHits { get; }

    /// <summary>
    /// The health colouring laid over his own colours in the band, or null where none is laid and they stay as they are:
    /// the absence of a colouring, not a colouring of its own.
    /// </summary>
    public RidleyHealthShade? HealthShade { get; }

    /// <summary>The band Ceres Ridley is in after <paramref name="hits"/> accepted hits.</summary>
    internal static CeresRidleyHealthBand Of(int hits) =>
        CeresRidleyHealthBandDefinitions.HighestFirst.First(band => hits >= band.MinimumHits);
}
