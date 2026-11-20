using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// A ceiling spore spawner in Spore Spawn's room (#1760, #2410): it releases spores on its random cadence while Spore Spawn
/// lets it, and a shot one restarts closed.
/// </summary>
/// <param name="room">The loaded room, whose random numbers time the releases.</param>
/// <param name="projectiles">The enemy projectiles, which hold the spores the spawner releases.</param>
/// <param name="deathsAndDrops">The killed sound and the drop a shot spore the spawner releases leaves.</param>
/// <param name="emitter">What the spawner keeps: the Spore Spawn that set it up, the way its spores drift and its release wait.</param>
internal sealed class SporeSpawnSpawnerBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyDeathsAndDrops deathsAndDrops,
    SporeSpawnEmitterState emitter)
    : EnemyProjectileMotionBehavior<SporeSpawnSpawnerMotion>(SporeSpawnSpawnerMotion.Releasing), IEnemyProjectileCues<SporeSpawnSpawnerCue>
{
    /// <inheritdoc/>
    internal override EnemyProjectileAnimation ShotAnimation() => new SporeSpawnSpawnerAnimation(SporeSpawnProjectileSegment.SpawnerClosed, this);

    /// <summary>Releases spores on the spawner's random cadence while Spore Spawn lets it.</summary>
    /// <param name="context">The spawner and what its pass hands every motion.</param>
    /// <param name="motion">The spawner's motion, its releasing, which only a spawner's spawn starts.</param>
    internal override void Move(EnemyProjectileMotionContext context, SporeSpawnSpawnerMotion motion)
    {
        switch (motion)
        {
            case SporeSpawnSpawnerMotion.Releasing:
                ReleaseSpores(context.Projectile);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SporeSpawnSpawnerMotion)} {motion}.");
        }
    }

    /// <summary>Carries out a cue of the spawner's release: the spore it spawns.</summary>
    public void RunCue(EnemyProjectileCueContext context, SporeSpawnSpawnerCue cue)
    {
        switch (cue)
        {
            case SporeSpawnSpawnerCue.SpawnSpore:
                // The spore takes the colours of the Spore Spawn that set the spawner up.
                SpawnSporeSpawnSpore(context.Projectile.Position.Whole, emitter.Drift, emitter.SporeSpawn);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SporeSpawnSpawnerCue)} {cue}.");
        }
    }

    /// <summary>
    /// Spawns one spore projectile and runs its setup. The spore shares <paramref name="sporeSpawn"/>'s body's colours as
    /// they show now (#1512).
    /// </summary>
    private void SpawnSporeSpawnSpore(Position16<RoomPixels> position, HorizontalDirection drift, SporeSpawnEnemyState sporeSpawn)
    {
        EnemyProjectile? spore = projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.SporeSpawnSpore, spores, SpriteGraphics.SharedIn(Ensure.NotNull(sporeSpawn).Colours.Sprites));
        if (spore is null)
            return;

        var behavior = new SporeSpawnSporeBehavior(deathsAndDrops, new SporeSpawnSporeState(drift));
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spore,
            RoomEnemyProjectileKind.SporeSpawnSpore,
            behavior,
            SpriteGraphics.SharedIn(Ensure.NotNull(sporeSpawn).Colours.Sprites));
        spore.StartAnimation(new SporeSpawnSporeAnimation(SporeSpawnProjectileSegment.Spore, behavior));
        spore.Position = spore.Position.WithWhole(position);
    }

    /// <summary>Releases spores from a ceiling spawner on its random cadence while Spore Spawn lets it.</summary>
    private void ReleaseSpores(EnemyProjectile spawner)
    {
        if (emitter.SporeSpawn.SporeGenerationSuppressed)
            return;

        if (emitter.ReleaseWait.IsZero)
        {
            spawner.StartAnimation(new SporeSpawnSpawnerAnimation(this, SporeSpawnProjectileSegment.SpawnerRelease, this));
            emitter.ReleaseWait = WrappingCountdown.Start(room.Random.Next(SporeSpawnRandomDefinitions.SporeReleaseWait));
        }
        emitter.ReleaseWait = emitter.ReleaseWait.Decrement().Remaining;
    }
}
