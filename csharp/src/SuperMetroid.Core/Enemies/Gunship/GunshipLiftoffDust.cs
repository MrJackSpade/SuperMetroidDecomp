using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The dust clouds the gunship's liftoff raises below Samus: their spawn into the room's enemy projectiles, and their
/// behaviour, which holds still and leaves when the cloud's animation ends.
/// </summary>
internal static partial class GunshipLiftoffDust
{
    /// <summary>
    /// Spawns one of the liftoff's six dust clouds, <paramref name="placement"/>, below <paramref name="samus"/> into
    /// <paramref name="projectiles"/>. A spawn that finds every place held spawns nothing.
    /// </summary>
    internal static void SpawnCloud(EnemyProjectileEngine projectiles, GunshipDustBehavior dust, GunshipDustPlacement placement, SamusState samus)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(samus);
        EnemyProjectile? projectile = projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GunshipLiftoffDustCloud, Ensure.NotNull(dust), graphics: SpriteGraphics.SharedStandard, new GunshipDustAnimation(dust, placement.Animation));
        if (projectile is null)
            return;

        projectile.Position = projectile.Position.WithWhole(new(samus.Position.X.Whole + placement.XOffset, samus.Position.Y.Whole + GunshipDustDefinitions.YOffset));
        projectile.Velocity = new(SignedEightEight.Zero, SignedEightEight.Zero);
    }

    /// <summary>A gunship liftoff dust cloud, which ends with its animation.</summary>
    internal sealed partial class GunshipDustBehavior : EnemyProjectileBehavior, IEnemyProjectileCues<GunshipDustCue>
    {
        /// <summary>What <see cref="RunCue"/> does for each <see cref="GunshipDustCue"/> member, each member's own arm.</summary>
        private static IdentityTable<GunshipDustCue, Action<EnemyProjectileCueContext>> RunCueForGunshipDustCue => field ??= new(new Dictionary<GunshipDustCue, Action<EnemyProjectileCueContext>>
        {
            [GunshipDustCue.Delete] = static context => context.Projectile.Delete(),
        });

        /// <summary>Deletes the dust cloud when its animation ends.</summary>
        public void RunCue(EnemyProjectileCueContext context, GunshipDustCue cue)
        {
            RunCueForGunshipDustCue[cue](context);
        }
    }
}
