using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>The old Tourian escape shaft's fake wall: the explosion that goes off as it breaks.</summary>
[RoomLifetime]
internal sealed class OldTourianEscapeShaftWallBehavior
{
    /// <summary>The room's enemy projectiles, which the wall's explosion spawns.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Enemy sound queue used by the wall explosion's cue.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds room, projectile, and audio owners needed by the old Tourian fake wall.</summary>
    internal OldTourianEscapeShaftWallBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Spawns the fake wall's explosion, on the middle of the breaking wall.
    /// </summary>
    public void SpawnOldTourianEscapeShaftWallExplosion()
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion, WallExplosion, graphics: SpriteGraphics.SharedStandard, new OldTourianWallExplosionAnimation(WallExplosion, OldTourianWallExplosionSegment.Start));
        if (projectile is null)
            return; // Spawning fails when the room has no place left for an enemy projectile.

        // The wall's projectile draws from the room's own graphics.
        projectile.Position = projectile.Position.WithWholeX(OldTourianEscapeShaftWallExplosionAnimationDefinitions.XPosition);
        projectile.Position = projectile.Position.WithWholeY(OldTourianEscapeShaftWallExplosionAnimationDefinitions.YPosition);
    }

    /// <summary>The fake wall's explosion (#1760): it holds still while its spawner's animation plays.</summary>
    private WallExplosionBehavior WallExplosion => field ??= new WallExplosionBehavior(_projectiles, _audio);

    /// <summary>
    /// The fake wall's explosion, which carries out its animation's cues: it scatters through the projectile engine and
    /// makes its sound through the enemy sound queue.
    /// </summary>
    /// <param name="projectiles">The enemy projectiles, which scatter the explosion.</param>
    /// <param name="audio">The enemy sound queue the explosion's sound joins.</param>
    internal sealed class WallExplosionBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
        : EnemyProjectileBehavior, IEnemyProjectileCues<OldTourianWallExplosionCue>
    {
        /// <summary>What <see cref="RunCue"/> does for each <see cref="OldTourianWallExplosionCue"/> member, each member's own arm.</summary>
        private IdentityTable<OldTourianWallExplosionCue, Action<EnemyProjectile>> RunCueForOldTourianWallExplosionCue => field ??= new(new Dictionary<OldTourianWallExplosionCue, Action<EnemyProjectile>>
        {
            [OldTourianWallExplosionCue.StopMoving] = projectile => projectile.HoldStill(),
            [OldTourianWallExplosionCue.Scatter] = projectile => projectiles.Scatter(projectile, OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter),
            [OldTourianWallExplosionCue.ExplosionSound] = projectile =>
            {
                audio.QueueEnemySound(
                    OldTourianEscapeShaftWallExplosionAnimationDefinitions.ExplosionSound,
                    OldTourianEscapeShaftWallExplosionAnimationDefinitions.ExplosionSoundPriority);
            },
            [OldTourianWallExplosionCue.Delete] = projectile => projectile.Delete(),
        });

        /// <summary>Carries out one cue of the wall explosion's animation.</summary>
        public void RunCue(EnemyProjectileCueContext context, OldTourianWallExplosionCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            RunCueForOldTourianWallExplosionCue[cue](projectile);
        }
    }
}
