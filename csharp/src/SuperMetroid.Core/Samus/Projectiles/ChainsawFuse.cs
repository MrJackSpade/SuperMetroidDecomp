using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Power Bomb fuse the Chainsaw shot, an uncharged Spazer, Plasma and Wave beam, runs over itself
/// (<see cref="ChainsawPowerBombShot"/>); its window's edges start where the fuse starts (#1407).
/// </summary>
internal sealed class ChainsawFuse
{
    /// <summary>Projectile animation owner used when the fuse ends the shot.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Where the shot's animation lands when the borrowed fuse speeds up.</summary>
    private readonly ChainsawFuseAnimationShift<SamusProjectileSegmentBeat> _fuseShift;

    /// <summary>Connects the fuse to projectile animation cleanup and the fuse's animation shift.</summary>
    internal ChainsawFuse(ProjectileAnimation animation,
        ChainsawFuseAnimationShift<SamusProjectileSegmentBeat> fuseShift)
    {
        _animation = animation;
        _fuseShift = Ensure.NotNull(fuseShift);
    }

    /// <summary>Runs the Power Bomb fuse over the shot, then the Power Bomb's block reactions.</summary>
    internal void RunPowerBombFuse(
        RoomLevelData level,
        SamusProjectile shot,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects roomObjects)
    {
        // The shot runs the Power Bomb fuse over itself, from a zero fuse.
        PowerBombFuseStep fuse = SamusPowerBombFuse.Step(shot.ChainsawPowerBombFuse, sharedProjectiles.PowerBombExplosion.IsArmed);
        shot.ChainsawPowerBombFuse = fuse.Timer;
        // On a beam the fuse's switch to the fast animation moves the beam's own animation as far as a bomb's fast
        // animation lies from its slow one, which lands wherever the game's animations lie there.
        if (fuse.FastAnimation && shot.Playback is { } live)
            shot.Playback = SamusProjectilePlayback.PlaybackAt(_fuseShift.Shift(SamusProjectilePlayback.BeatOf(live)));

        // The borrowed fuse starts the explosion as a Power Bomb's does, its sound included.
        if (fuse.SpawnExplosion)
            sharedProjectiles.SpawnPowerBombExplosion(shot.Position.Whole);
        if (fuse.DeleteProjectile)
            _animation.ClearProjectile(shot);

        // The Power Bomb's block reactions follow the fuse whether or not it cleared the shot, at the live Power Bomb's
        // centre and radius, not the Chainsaw shot's position; the blocks on the radius's edge react too, and the room
        // objects they set off change at once.
        SamusBombProjectileSystem.ReactToPowerBombBoundary(
            level,
            sharedProjectiles.PowerBombExplosion.Position,
            sharedProjectiles.PowerBombExplosion.Blast.ExplosionRadius,
            roomObjects,
            shot.Type);
    }
}
