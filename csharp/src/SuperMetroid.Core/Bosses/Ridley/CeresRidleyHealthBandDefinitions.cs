namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's health bands (#2242), by accepted hits. He has no badly wounded band: from <see cref="NearlyDead"/>'s
/// first hit on he shows the nearly-dead colouring.
/// </summary>
internal static class CeresRidleyHealthBandDefinitions
{
    /// <summary>Ceres Ridley before he is wounded: his own colours.</summary>
    internal static CeresRidleyHealthBand Unhurt { get; } = new(minimumHits: null, healthShade: null);

    /// <summary>Ceres Ridley once wounded, until he is nearly dead: the wounded colouring.</summary>
    internal static CeresRidleyHealthBand Wounded { get; } = new(minimumHits: 50, RidleyHealthShadeDefinitions.Wounded);

    /// <summary>Ceres Ridley once nearly dead: the nearly-dead colouring.</summary>
    internal static CeresRidleyHealthBand NearlyDead { get; } = new(minimumHits: 70, RidleyHealthShadeDefinitions.NearlyDead);

    /// <summary>The bands, highest first: the order his count of accepted hits is tested against them.</summary>
    internal static IReadOnlyList<CeresRidleyHealthBand> HighestFirst { get; } = [NearlyDead, Wounded, Unhurt];
}
