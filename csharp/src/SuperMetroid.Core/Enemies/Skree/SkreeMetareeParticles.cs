using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The particles a Skree or Metaree bursts into as it burrows or dies: their spawn into the room's enemy projectiles and
/// their flight under gravity until they leave the camera.
/// </summary>
internal static class SkreeMetareeParticles
{

    /// <summary>A Skree's or Metaree's burst particle (#1760): it flies under gravity until it leaves the camera.</summary>
    private static EnemyProjectileBehavior Behavior { get; } = new SingleMotionProjectileBehavior(
        context => Fly(context.Projectile, context.Camera));

    /// <summary>
    /// Spawns the four particles a Skree or Metaree bursts into, showing its <paramref name="look"/>, into
    /// <paramref name="projectiles"/> either side of <paramref name="source"/>, the left pair mirroring the right pair's
    /// launch (<see cref="MetalParticleBurstDefinitions"/>). Each enemy's particles are its own: they show its look and draw
    /// from its own spawned-object sheet (#1435).
    /// </summary>
    internal static void SpawnBurst(EnemyProjectileEngine projectiles, RoomEnemyOccupant occupant, RoomEnemyPlace source, SkreeMetareeParticleSegment look)
    {
        Ensure.NotNull(projectiles);
        SpawnParticle(projectiles, occupant, source, look, MetalParticleBurstDefinitions.SideOffset, MetalParticleBurstDefinitions.OuterRightVelocity);
        SpawnParticle(projectiles, occupant, source, look, MetalParticleBurstDefinitions.SideOffset, MetalParticleBurstDefinitions.InnerRightVelocity);
        SpawnParticle(projectiles, occupant, source, look, -MetalParticleBurstDefinitions.SideOffset, MetalParticleBurstDefinitions.OuterRightVelocity.ReversedX());
        SpawnParticle(projectiles, occupant, source, look, -MetalParticleBurstDefinitions.SideOffset, MetalParticleBurstDefinitions.InnerRightVelocity.ReversedX());
    }

    /// <summary>Flies a particle under gravity, removing it once it leaves the view, whose right and bottom edges are already outside.</summary>
    private static void Fly(EnemyProjectile projectile, Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToDisplacement());
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + MetalParticleBurstDefinitions.Gravity);
        if (!EnemyView.ContainsBeforeFarEdges(projectile.Position.Whole, camera))
            projectile.Delete();
    }

    /// <summary>
    /// Spawns one particle through the projectile spawn's definition copy (its damage, contact enabled, Skree or Metaree
    /// animation), then applies the shared particle setup's position and velocity. A spawn that finds
    /// every place held spawns nothing.
    /// </summary>
    private static void SpawnParticle(
        EnemyProjectileEngine projectiles,
        RoomEnemyOccupant occupant, RoomEnemyPlace source,
        SkreeMetareeParticleSegment look,
        int xOffset,
        PixelVelocity<RoomPixels> velocity)
    {
        EnemyProjectile? particle = projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.SkreeMetareeParticle, Behavior, SpriteGraphics.SpawnedBy(occupant, source.Shading), new SkreeMetareeParticleAnimation(look));
        if (particle is null)
            return;

        particle.Position = particle.Position.WithWhole(new(source.Position.X.Whole + xOffset, source.Position.Y.Whole));
        particle.Velocity = velocity;
    }
}
