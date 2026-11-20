using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// A ceiling spore spawner in Spore Spawn's room (#1760, #2410): it releases spores on its random cadence while Spore Spawn
/// lets it, each spore running <paramref name="spores"/>, and a shot one restarts closed.
/// </summary>
/// <param name="room">The loaded room, whose random numbers time the releases.</param>
/// <param name="projectiles">The enemy projectiles, which hold the spores the spawner releases.</param>
/// <param name="spores">The behaviour every spore the spawner releases runs.</param>
internal sealed class SporeSpawnSpawnerBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, SporeSpawnSporeBehavior spores)
    : EnemyProjectileMotionBehavior<SporeSpawnSpawnerMotion>(SporeSpawnSpawnerMotion.Releasing), IEnemyProjectileCues<SporeSpawnProjectileCue>
{
    /// <summary>The Spore Spawn that set each spawner up and the way its spores drift, given as it opens.</summary>
    internal EnemyProjectileStates<SporeSpawnEmitterState> Emitters { get; } = new();

    /// <inheritdoc/>
    internal override EnemyProjectileAnimation ShotAnimation() =>
        new SporeSpawnProjectileAnimation(this, SporeSpawnProjectileSegment.SpawnerClosed);

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
    public void RunCue(EnemyProjectileCueContext context, SporeSpawnProjectileCue cue)
    {
        switch (cue)
        {
            case SporeSpawnProjectileCue.SpawnSpore:
                // The spore takes the colours of the Spore Spawn that set the spawner up.
                EnemyProjectile spawner = context.Projectile;
                SporeSpawnEmitterState emitter = Emitters.Of(spawner);
                SpawnSporeSpawnSpore(spawner.Position.Whole, emitter.Drift, emitter.SporeSpawn);
                return;
            case SporeSpawnProjectileCue.SetImpactProperties or SporeSpawnProjectileCue.QueueKilledSound or SporeSpawnProjectileCue.RequestDrop
                or SporeSpawnProjectileCue.Delete:
                throw new InvalidOperationException($"A ceiling spore spawner's animations publish no {cue}.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(SporeSpawnProjectileCue)} {cue}.");
        }
    }

    /// <summary>
    /// Spawns one spore projectile and runs its setup. The spore shares <paramref name="sporeSpawn"/>'s body's colours as
    /// they show now (#1512).
    /// </summary>
    private void SpawnSporeSpawnSpore(Position16<RoomPixels> position, HorizontalDirection drift, SporeSpawnEnemyState sporeSpawn)
    {
        EnemyProjectile? spore = projectiles.AllocateEnemyProjectile();
        if (spore is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spore,
            RoomEnemyProjectileKind.SporeSpawnSpore,
            spores,
            SpriteGraphics.SharedIn(Ensure.NotNull(sporeSpawn).Colours.Sprites));
        spore.StartAnimation(new SporeSpawnProjectileAnimation(spores, SporeSpawnProjectileSegment.Spore));
        spore.Position = spore.Position.WithWhole(position);

        spores.Drifts.Start(spore, new SporeSpawnSporeState(drift));
    }

    /// <summary>Releases spores from a ceiling spawner on its random cadence while Spore Spawn lets it.</summary>
    private void ReleaseSpores(EnemyProjectile spawner)
    {
        SporeSpawnEmitterState emitter = Emitters.Of(spawner);
        if (emitter.SporeSpawn.SporeGenerationSuppressed)
            return;

        if (emitter.ReleaseWait.IsZero)
        {
            spawner.StartAnimation(new SporeSpawnProjectileAnimation(this, SporeSpawnProjectileSegment.SpawnerRelease));
            emitter.ReleaseWait = WrappingCountdown.Start(room.Random.Next(SporeSpawnRandomDefinitions.SporeReleaseWait));
        }
        emitter.ReleaseWait = emitter.ReleaseWait.Decrement().Remaining;
    }
}
