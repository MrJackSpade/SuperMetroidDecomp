namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's three health bands (#2242), by accepted hits. He has no badly wounded band: from his seventieth hit on
/// he shows the nearly-dead colouring.
/// </summary>
internal static class CeresRidleyHealthBandDefinitions
{
    /// <summary>Ceres Ridley before his fiftieth accepted hit: his own colours.</summary>
    internal static CeresRidleyHealthBand Unhurt { get; } = new(minimumHits: 0, healthShade: null);

    /// <summary>Ceres Ridley from his fiftieth to his sixty-ninth accepted hit: the wounded colouring.</summary>
    internal static CeresRidleyHealthBand Wounded { get; } = new(minimumHits: 50, RidleyHealthShadeDefinitions.Wounded);

    /// <summary>Ceres Ridley from his seventieth accepted hit on: the nearly-dead colouring.</summary>
    internal static CeresRidleyHealthBand NearlyDead { get; } = new(minimumHits: 70, RidleyHealthShadeDefinitions.NearlyDead);

    /// <summary>The bands, highest first: the order his count of accepted hits is tested against them.</summary>
    internal static IReadOnlyList<CeresRidleyHealthBand> HighestFirst { get; } = [NearlyDead, Wounded, Unhurt];
}
