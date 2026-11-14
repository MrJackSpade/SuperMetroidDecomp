using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>The old Tourian escape-shaft wall species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class OldTourianEscapeShaftWallBehavior
{
    /// <summary>Loaded-room dependency required before the wall controller can spawn its explosion.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Eighteen-slot enemy projectile allocator for the wall explosion.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Enemy sound queue used by the wall explosion's cue.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds room, projectile, and audio owners needed by the old Tourian fake-wall controller.</summary>
    internal OldTourianEscapeShaftWallBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Spawns the fake-wall explosion for the wall's frame behaviour. Its initializer
    /// ignores the spawn parameter and places the explosion on the middle of the
    /// breaking wall.
    /// </summary>
    public void SpawnOldTourianEscapeShaftWallExplosion()
    {
        _room.EnsureLoaded();
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // Spawning fails when all eighteen projectile slots are live.

        // The room-graphics enemy-projectile spawn uses graphics index zero.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion,
            WallExplosion,
            graphics: default);
        projectile.StartAnimation(new OldTourianWallExplosionAnimation(OldTourianWallExplosionSegment.Start));
        projectile.Position = projectile.Position.WithWholeX(OldTourianEscapeShaftWallExplosionAnimationDefinitions.XPosition);
        projectile.Position = projectile.Position.WithWholeY(OldTourianEscapeShaftWallExplosionAnimationDefinitions.YPosition);
    }

    /// <summary>The fake wall's explosion (#1760): it holds still while its spawner's animation plays.</summary>
    internal EnemyProjectileBehavior WallExplosion => field ??= new WallExplosionBehavior(_projectiles, _audio);

    /// <summary>
    /// The fake wall's explosion, which carries out its animation's cues: it scatters through the projectile engine and
    /// makes its sound through the enemy sound queue.
    /// </summary>
    /// <param name="projectiles">The enemy projectiles, which scatter the explosion.</param>
    /// <param name="audio">The enemy sound queue the explosion's sound joins.</param>
    private sealed class WallExplosionBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
        : EnemyProjectileBehavior, IEnemyProjectileCues<OldTourianWallExplosionCue>
    {
        /// <summary>Carries out one cue of the wall explosion's animation.</summary>
        public void RunCue(EnemyProjectileCueContext context, OldTourianWallExplosionCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case OldTourianWallExplosionCue.ClearFrameBehavior:
                    projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                    break;
                case OldTourianWallExplosionCue.Scatter:
                    projectiles.Scatter(projectile, OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter);
                    break;
                case OldTourianWallExplosionCue.ExplosionSound:
                    audio.QueueEnemySound(
                        OldTourianEscapeShaftWallExplosionAnimationDefinitions.ExplosionSound,
                        OldTourianEscapeShaftWallExplosionAnimationDefinitions.ExplosionSoundPriority);
                    break;
                case OldTourianWallExplosionCue.Delete:
                    projectile.Delete();
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(OldTourianWallExplosionCue)} {cue}.");
            }
        }
    }
}
