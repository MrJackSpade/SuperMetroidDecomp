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
            graphics: default);
        projectile.StartAnimation(new OldTourianWallExplosionAnimation(OldTourianWallExplosionSegment.Start));
        projectile.Position = projectile.Position.WithWholeX(OldTourianEscapeShaftWallExplosionAnimationDefinitions.XPosition);
        projectile.Position = projectile.Position.WithWholeY(OldTourianEscapeShaftWallExplosionAnimationDefinitions.YPosition);
    }


    /// <summary>Registers the handler that carries out the wall explosion animation's cues.</summary>
    internal void AddWallExplosionCues(EnemyProjectileHandlerTable table) =>
        table.AddCue<OldTourianWallExplosionCue>((context, cue) => ApplyWallExplosionCue(context.Projectile, cue));

    /// <summary>Carries out one cue of the wall explosion's animation.</summary>
    private void ApplyWallExplosionCue(EnemyProjectile projectile, OldTourianWallExplosionCue cue)
    {
        switch (cue)
        {
            case OldTourianWallExplosionCue.ClearFrameBehavior:
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                break;
            case OldTourianWallExplosionCue.Scatter:
                _projectiles.Scatter(projectile, OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter);
                break;
            case OldTourianWallExplosionCue.ExplosionSound:
                _audio.QueueEnemySound(
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
