using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What one of Spore Spawn's spores keeps: which way its drift is mirrored, and how far along its drift path it is.</summary>
/// <param name="mirrored">Whether the spore drifts the opposite way to the authored path.</param>
internal sealed class SporeSpawnSporeState(bool mirrored) : EnemyProjectileState
{
    /// <summary>How many steps the drift path holds before it repeats.</summary>
    private const int PathSteps = 128;

    /// <summary>Whether the spore drifts the opposite way to the authored path: its emitter sits in the right half of a screen.</summary>
    internal bool Mirrored { get; } = mirrored;

    /// <summary>The spore's step along its drift path, 0 through 127.</summary>
    internal int PathStep
    {
        get;
        set => field = Ensure.BetweenInclusive(0, PathSteps - 1, value);
    }

    /// <summary>Moves the spore on to the next step of its path, repeating the path after its last step.</summary>
    internal void AdvancePath() => PathStep = (PathStep + 1) % PathSteps;
}
