namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy a death explosion or its drop came from, and whether that enemy respawns from its snapshot when the
/// explosion's respawn cue runs (#1412). The enemy is its place, so the respawn rebuilds whatever the place's snapshot
/// holds (#1542). A drop spawned directly came from no enemy, and has none.
/// </summary>
public sealed record KilledEnemy
{
    /// <summary>Keeps the killed enemy's place and whether it respawns.</summary>
    private KilledEnemy(RoomEnemyPlace enemy, bool respawns)
    {
        Enemy = enemy;
        Respawns = respawns;
    }

    /// <summary>The enemy in <paramref name="place"/>, which respawns when the explosion ends if <paramref name="respawns"/>.</summary>
    /// <param name="place">The killed enemy's place.</param>
    /// <param name="respawns">Whether the explosion's respawn cue rebuilds the enemy from its snapshot.</param>
    public static KilledEnemy Of(RoomEnemyPlace place, bool respawns) => new(Ensure.NotNull(place), respawns);

    /// <summary>The killed enemy's place.</summary>
    public RoomEnemyPlace Enemy { get; }

    /// <summary>Whether the explosion's respawn cue rebuilds the enemy from its snapshot.</summary>
    public bool Respawns { get; }
}
