namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A death explosion, or the pickup it becomes, that came from a killed enemy (#2455): the drop's closing tail rebuilds
/// that enemy when it respawns. A pickup spawned directly came from no enemy and is no such state.
/// </summary>
internal interface IFromKilledEnemy
{
    /// <summary>The enemy the explosion came from, and whether the drop's closing tail rebuilds it.</summary>
    KilledEnemy KilledEnemy { get; }
}
