using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The particles a Skree or Metaree bursts into as it burrows or dies: their spawn into the room's enemy projectiles and
/// their flight under gravity until they leave the camera.
/// </summary>
internal static class SkreeMetareeParticles
{
    /// <summary>A particle's downward acceleration per update, in 256ths of a pixel per update.</summary>
    private static readonly SignedEightEight Gravity = SignedEightEight.FromParts(0, 80);

    /// <summary>A Skree's or Metaree's burst particle (#1760): it flies under gravity until it leaves the camera.</summary>
    internal static EnemyProjectileBehavior Behavior { get; } = new SingleMotionProjectileBehavior(
        EnemyProjectileFrameBehavior.SkreeParticleFlight, context => Fly(context.Projectile, context.Camera));

    /// <summary>
    /// Spawns the four particles of <paramref name="burst"/> into <paramref name="projectiles"/>, six pixels either side of
    /// <paramref name="source"/>: the outer pair flies at 1.25 pixels outward and just over 3 up per update, the inner pair
    /// at 0.375 outward and just over 4 up, in 1/256 pixels.
    /// </summary>
    internal static void SpawnBurst(EnemyProjectileEngine projectiles, RoomEnemySlot source, MetalParticleBurst burst)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(burst);
        SpawnParticle(projectiles, source, burst.DownRight, burst.Look, 6, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(320, -769));
        SpawnParticle(projectiles, source, burst.UpRight, burst.Look, 6, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(96, -1025));
        SpawnParticle(projectiles, source, burst.DownLeft, burst.Look, -6, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-320, -769));
        SpawnParticle(projectiles, source, burst.UpLeft, burst.Look, -6, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-96, -1025));
    }

    /// <summary>Flies a particle under gravity, removing it once it leaves the camera's square.</summary>
    private static void Fly(EnemyProjectile projectile, Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToSixteenSixteen());
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + Gravity);
        if (CoordinateAxis.Wrap(projectile.Position.X.Whole - camera.X) >= 256 ||
            CoordinateAxis.Wrap(projectile.Position.Y.Whole - camera.Y) >= 256)
        {
            projectile.Delete();
        }
    }

    /// <summary>
    /// Spawns one particle through the projectile spawn's definition copy (damage four, contact enabled, Skree or Metaree
    /// animation), then applies the shared particle initializer's position and 1/256-pixel velocity. A spawn that finds
    /// every place held spawns nothing.
    /// </summary>
    private static void SpawnParticle(
        EnemyProjectileEngine projectiles,
        RoomEnemySlot source,
        RoomEnemyProjectileKind kind,
        SkreeMetareeParticleSegment look,
        int xOffset,
        EightEightVector<RoomPixels> velocity)
    {
        EnemyProjectile? particle = projectiles.AllocateEnemyProjectile();
        if (particle is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            particle,
            kind,
            Behavior,
            SpriteGraphics.SpawnedBy(source));
        particle.StartAnimation(new SkreeMetareeParticleAnimation(look));
        particle.Position = particle.Position.WithWhole(new(CoordinateAxis.Wrap(source.Position.X.Whole + xOffset), source.Position.Y.Whole));
        particle.Velocity = velocity;
    }
}
