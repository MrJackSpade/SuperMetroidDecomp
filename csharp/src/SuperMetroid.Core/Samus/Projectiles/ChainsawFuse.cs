using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Power Bomb fuse the Chainsaw shot, an uncharged Spazer, Plasma and Wave beam, runs over itself
/// (<see cref="ChainsawPowerBombShot"/>); when the fuse runs out, a Power Bomb blast starts at the shot (#1407).
/// </summary>
/// <remarks>
/// The fuse is the one the beam carries (<see cref="SamusProjectile.BeamFuse"/>). A shot fired into a free place carries
/// none, so its fuse has burned out before it starts and the shot is deleted on its first update unless a Power Bomb's
/// blast is under way. A shot fired with every place held over the shot in the last place starts from the count that shot
/// leaves (<see cref="BeamFiredOverLastShot"/>, #2468): it counts that down, moves its animation at the fast stage
/// (<see cref="ChainsawFuseAnimationShift{TCursor}"/>) and starts the Power Bomb blast when it runs out.
/// </remarks>
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
        PowerBombFuseStep fuse = SamusPowerBombFuse.Step(shot.BeamFuse, sharedProjectiles.PowerBombExplosion.IsArmed);
        shot.BeamFuse = fuse.UpdatesLeft;
        // On a beam the fuse's switch to the fast animation moves the beam's own animation as far as a bomb's fast
        // animation lies from its slow one, which lands wherever the game's animations lie there.
        if (fuse.FastAnimation && shot.Animation is { } live)
            live.GoOnFrom(SamusProjectilePlayback.PlaybackAt(_fuseShift.Shift(SamusProjectilePlayback.BeatOf(live.Playback))));

        // The borrowed fuse starts the explosion as a Power Bomb's does, its sound included.
        if (fuse.SpawnExplosion)
            sharedProjectiles.SpawnPowerBombExplosion(shot.Position.Whole);
        if (fuse.DeleteProjectile)
            _animation.ClearProjectile(shot);

        // The Power Bomb's block reactions follow a burned-out fuse whether or not it cleared the shot, at the live Power
        // Bomb's centre and radius, not the Chainsaw shot's position; the blocks on the radius's edge react too, and the
        // room objects they set off change at once. A fuse still burning, or burning out this update, reacts with nothing.
        if (fuse.BoundaryReacts)
        {
            SamusBombProjectileSystem.ReactToPowerBombBoundary(
                level,
                sharedProjectiles.PowerBombExplosion.Position,
                sharedProjectiles.PowerBombExplosion.Blast.ExplosionRadius,
                roomObjects,
                shot.Type);
        }
    }
}
