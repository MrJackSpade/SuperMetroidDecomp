using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// A spore a ceiling spawner releases in Spore Spawn's room (#1760, #2410): it drifts along its path from its spawn until it
/// falls below the room, and a shot one bursts, harmless to Samus, then queues the killed sound, leaves its drop and
/// deletes itself.
/// </summary>
/// <param name="deathsAndDrops">The killed sound and the drop a shot spore leaves.</param>
internal sealed class SporeSpawnSporeBehavior(EnemyDeathsAndDrops deathsAndDrops)
    : EnemyProjectileMotionBehavior<SingleMotion>(SingleMotion.Moving), IEnemyProjectileCues<SporeSpawnProjectileCue>
{
    /// <summary>The way each spore drifts and its step along its path, given as its spawner releases it.</summary>
    internal EnemyProjectileStates<SporeSpawnSporeState> Drifts { get; } = new();

    /// <inheritdoc/>
    internal override EnemyProjectileAnimation ShotAnimation() =>
        new SporeSpawnProjectileAnimation(this, SporeSpawnProjectileSegment.SporeShot);

    /// <summary>Drifts the spore one step along its path.</summary>
    /// <param name="context">The spore and what its pass hands every motion.</param>
    /// <param name="motion">The spore's one motion, its drift.</param>
    internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) => DriftSpore(context.Projectile);

    /// <summary>Carries out a cue of a shot spore's burst: its impact properties, the killed sound, its drop and its deletion.</summary>
    public void RunCue(EnemyProjectileCueContext context, SporeSpawnProjectileCue cue)
    {
        EnemyProjectile spore = context.Projectile;
        switch (cue)
        {
            case SporeSpawnProjectileCue.SetImpactProperties:
                SetImpactProperties(spore);
                return;
            case SporeSpawnProjectileCue.QueueKilledSound:
                deathsAndDrops.QueueEnemyKilledSound();
                return;
            case SporeSpawnProjectileCue.RequestDrop:
                deathsAndDrops.SpawnEnemyDropFromChanceTable(spore.Position.Whole, SporeSpawnProjectileDefinitions.SporeDropChances, context.Samus);
                return;
            case SporeSpawnProjectileCue.Delete:
                spore.Delete();
                return;
            case SporeSpawnProjectileCue.SpawnSpore:
                throw new InvalidOperationException("A spore's animations release no spores.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(SporeSpawnProjectileCue)} {cue}.");
        }
    }

    /// <summary>Drifts a spore one step along its path, deleting it once it falls below the room.</summary>
    internal void DriftSpore(EnemyProjectile spore)
    {
        SporeSpawnSporeState drift = Drifts.Of(Ensure.NotNull(spore));
        Offset16<RoomPixels> movement = drift.PathStep.Entry;
        int xDelta = movement.X;
        if (drift.Drift == HorizontalDirection.Left)
            xDelta = -xDelta;
        spore.Position = spore.Position.WithWholeX(spore.Position.X.Whole + xDelta);

        spore.Position = spore.Position.WithWholeY(spore.Position.Y.Whole + movement.Y);
        if (spore.Position.Y.Whole >= SporeSpawnArenaDefinitions.SporeFloorY)
            spore.Delete();

        drift.PathStep = drift.PathStep.NextInCycle;
    }

    /// <summary>A shot spore's impact step, which replaces its whole property set.</summary>
    private static void SetImpactProperties(EnemyProjectile spore)
    {
        // A shot spore does no damage and is gone when it touches Samus, and Samus's shots pass through it.
        spore.Damage = 0;
        spore.CanDamageSamus = true;
        spore.PersistsOnSamusContact = false;
        spore.BlocksSamusProjectiles = false;
    }
}
