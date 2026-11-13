namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy a death explosion or its drop came from, and whether that enemy respawns from its snapshot when the
/// explosion's respawn cue runs (#1412). The enemy is its place, so the respawn rebuilds whatever the place's snapshot
/// holds (#1542). A cleared projectile names no enemy and does not respawn.
/// </summary>
public readonly record struct KilledEnemy
{
    /// <summary>No killed enemy: a drop spawned directly rather than from a death.</summary>
    public static KilledEnemy None => default;

    /// <summary>The enemy in <paramref name="slot"/>, which respawns when the explosion ends if <paramref name="respawns"/>.</summary>
    /// <param name="slot">The killed enemy's place.</param>
    /// <param name="respawns">Whether the explosion's respawn cue rebuilds the enemy from its snapshot.</param>
    public static KilledEnemy Of(RoomEnemySlot slot, bool respawns) => new() { Enemy = Ensure.NotNull(slot), Respawns = respawns };

    /// <summary>The killed enemy's place; null for <see cref="None"/> and for a cleared projectile.</summary>
    public RoomEnemySlot? Enemy { get; private init; }

    /// <summary>Whether the explosion's respawn cue rebuilds the enemy from its snapshot.</summary>
    public bool Respawns { get; private init; }
}
