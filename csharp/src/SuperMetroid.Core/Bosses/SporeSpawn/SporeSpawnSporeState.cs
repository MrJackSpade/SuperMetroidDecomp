using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What one of Spore Spawn's spores keeps: the way it first drifts, and how far along its drift path it is.</summary>
/// <param name="drift">The way the spore first drifts, its emitter's.</param>
internal sealed class SporeSpawnSporeState(HorizontalDirection drift) : EnemyProjectileState
{
    /// <summary>The way the spore first drifts: right along its drift path as authored, left along it mirrored.</summary>
    internal HorizontalDirection Drift { get; } = drift;

    /// <summary>The spore's step along its drift path.</summary>
    internal int PathStep
    {
        get;
        set => field = Ensure.BetweenInclusive(0, SporeSpawnProjectiles.DriftStepCount - 1, value);
    }

    /// <summary>Moves the spore on to the next step of its path, repeating the path after its last step.</summary>
    internal void AdvancePath() => PathStep = (PathStep + 1) % SporeSpawnProjectiles.DriftStepCount;
}
