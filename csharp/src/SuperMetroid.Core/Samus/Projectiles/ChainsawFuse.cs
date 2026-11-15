using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Power Bomb fuse the Chainsaw shot, an uncharged Spazer, Plasma and Wave beam, runs over itself
/// (<see cref="ChainsawPowerBombShot"/>). The original's misaligned store into the cached window edges as it starts is a
/// presentation quirk the model drops (#1407).
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
        ISamusRoomObjects? roomObjects)
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

        // The fuse always falls through to the Power Bomb's block reactions, even when the zero-timer/no-flag branch just
        // cleared the projectile. The latter case consequently runs as no projectile, while both paths use the live Power
        // Bomb's centre and radius rather than the Chainsaw's position. Preserve the inclusive edge traversal and its
        // synchronous room-object mutations.
        var reactions = new List<BombBlockReaction>();
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            level,
            sharedProjectiles.PowerBombExplosion.Position,
            sharedProjectiles.PowerBombExplosion.Blast.ExplosionRadius,
            reactions,
            roomObjects,
            shot.Type);
    }
}
