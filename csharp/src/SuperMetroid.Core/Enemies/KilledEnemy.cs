namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy a death explosion or its drop came from, and whether that enemy respawns from its snapshot when the
/// explosion's respawn cue runs (#1412). The enemy is its place, so the respawn rebuilds whatever the place's snapshot
/// holds (#1542). A drop spawned directly came from no enemy, and has none.
/// </summary>
public sealed record KilledEnemy
{
    /// <summary>Keeps the killed enemy's place and whether it respawns.</summary>
    private KilledEnemy(PopulatedEnemyPlace enemy, bool respawns)
    {
        Enemy = enemy;
        Respawns = respawns;
    }

    /// <summary>The enemy killed in <paramref name="place"/>, which respawns when the explosion ends if <paramref name="respawns"/>.</summary>
    /// <param name="place">The history of the killed enemy's place, which keeps the entry a respawn rebuilds it from.</param>
    /// <param name="respawns">Whether the explosion's respawn cue rebuilds the enemy from its snapshot.</param>
    public static KilledEnemy Of(PopulatedEnemyPlace place, bool respawns) => new(Ensure.NotNull(place), respawns);

    /// <summary>The history of the killed enemy's place.</summary>
    public PopulatedEnemyPlace Enemy { get; }

    /// <summary>Whether the explosion's respawn cue rebuilds the enemy from its snapshot.</summary>
    public bool Respawns { get; }
}
