using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where Mother Brain's eleven Rinka spawn points lie (Japan/USA layout), in the order a moving Rinka tries them.</summary>
internal static class RinkaSpawnPointDefinitions
{
    /// <summary>
    /// Every spawn point with its centre, in the order a Rinka that cannot keep its own point tries them. A population
    /// spawn at no point's centre counts as the first point.
    /// </summary>
    internal static ImmutableArray<RinkaSpawnPointDefinition> SearchOrder { get; } =
    [
        new(RinkaSpawnPoint.FarRightUpper, new(999, 38)),
        new(RinkaSpawnPoint.FarRightLower, new(999, 166)),
        new(RinkaSpawnPoint.RightUpper, new(823, 54)),
        new(RinkaSpawnPoint.RightLower, new(823, 166)),
        new(RinkaSpawnPoint.CentreUpper, new(631, 28)),
        new(RinkaSpawnPoint.CentreLower, new(631, 182)),
        new(RinkaSpawnPoint.LeftUpper, new(439, 54)),
        new(RinkaSpawnPoint.LeftLower, new(439, 166)),
        new(RinkaSpawnPoint.FarLeftUpper, new(247, 28)),
        new(RinkaSpawnPoint.FarLeftLower, new(247, 182)),
        new(RinkaSpawnPoint.Leftmost, new(128, 168)),
    ];
}
