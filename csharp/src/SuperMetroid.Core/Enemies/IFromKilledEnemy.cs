namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A death explosion that came from a killed enemy (#2455): that enemy respawns when the drop ends if it is one that
/// respawns. A pickup came from no enemy and is no such state.
/// </summary>
internal interface IFromKilledEnemy
{
    /// <summary>The enemy the explosion came from, and whether it respawns when the drop ends.</summary>
    KilledEnemy KilledEnemy { get; }
}
