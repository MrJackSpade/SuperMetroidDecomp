using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The dust clouds the gunship's liftoff raises below Samus: their spawn into the room's enemy projectiles, and their
/// behaviour, which holds still and leaves when the cloud's animation ends.
/// </summary>
internal static class GunshipLiftoffDust
{
    /// <summary>The gunship's liftoff dust clouds (#1760): each holds still and leaves when its animation ends.</summary>
    internal static EnemyProjectileBehavior Behavior => Dust;

    /// <summary>The dust cloud's behaviour, which owns its animation's cues.</summary>
    private static GunshipDustBehavior Dust { get; } = new();

    /// <summary>
    /// Spawns one of the liftoff's six dust clouds, <paramref name="placement"/>, below <paramref name="samus"/> into
    /// <paramref name="projectiles"/>. A spawn that finds every place held spawns nothing.
    /// </summary>
    internal static void SpawnCloud(EnemyProjectileEngine projectiles, GunshipDustPlacement placement, SamusState samus)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(samus);
        EnemyProjectile? projectile = projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GunshipLiftoffDustCloud, Behavior, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;

        projectile.Position = projectile.Position.WithWhole(new(samus.Position.X.Whole + placement.XOffset, samus.Position.Y.Whole + GunshipDustDefinitions.YOffset));
        projectile.Velocity = new(SignedEightEight.Zero, SignedEightEight.Zero);
        projectile.StartAnimation(new GunshipDustAnimation(placement.Animation, Dust));
    }

    /// <summary>A gunship liftoff dust cloud, which ends with its animation.</summary>
    private sealed class GunshipDustBehavior : EnemyProjectileBehavior, IEnemyProjectileCues<GunshipDustCue>
    {
        /// <summary>Deletes the dust cloud when its animation ends.</summary>
        public void RunCue(EnemyProjectileCueContext context, GunshipDustCue cue)
        {
            switch (cue)
            {
                case GunshipDustCue.Delete:
                    context.Projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(GunshipDustCue)} {cue}.");
            }
        }
    }
}
