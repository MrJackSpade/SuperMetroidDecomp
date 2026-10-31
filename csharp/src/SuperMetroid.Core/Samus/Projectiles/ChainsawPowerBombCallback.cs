using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Chainsaw callback, reached by uncharged beam combination thirteen: it runs the Power Bomb pre-instruction over the
/// shot. Its misaligned store into the cached window registers is a presentation quirk the port drops (#1407).
/// </summary>
internal sealed class ChainsawPowerBombCallback
{
    /// <summary>Projectile animation owner used when the callback clears its slot.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Where the shot's animation lands when the borrowed fuse speeds up.</summary>
    private readonly ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> _fuseShift;

    /// <summary>Connects the callback to projectile animation cleanup and the fuse's animation shift.</summary>
    internal ChainsawPowerBombCallback(ProjectileAnimation animation,
        ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> fuseShift)
    {
        _animation = animation;
        _fuseShift = Ensure.NotNull(fuseShift);
    }

    /// <summary>Runs the Power Bomb fuse over the shot, then the Power Bomb boundary reactions.</summary>
    internal void RunPowerBombPreInstruction(
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSlot slot,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomPlms? roomPlms)
    {
        // The misaligned callback runs the Power Bomb fuse over the shot, from a zero fuse.
        PowerBombFuseStep fuse = SamusPowerBombFuse.Step(slot.ChainsawPowerBombFuse, sharedProjectiles.PowerBombExplosion.IsArmed);
        slot.ChainsawPowerBombFuse = fuse.Timer;
        // On a beam the fuse's switch to the fast animation moves the beam's own animation as far as a bomb's fast
        // animation lies from its slow one, which lands wherever the original game laid an animation out there.
        if (fuse.FastAnimation && slot.Cursor is { } live)
            slot.Cursor = _fuseShift.Shift(live);

        if (fuse.SpawnExplosion)
            sharedProjectiles.PowerBombExplosion.Spawn(slot.Position.Whole);
        if (fuse.DeleteProjectile)
            _animation.ClearProjectile(slot);

        // The callback always falls through from the fuse to the Power Bomb boundary reactions, even when the
        // zero-timer/no-flag branch just cleared the projectile. The latter case consequently dispatches as no
        // projectile, while both paths use the global Power Bomb centre/radius rather than the Chainsaw's position.
        // Preserve the inclusive edge traversal and its synchronous PLM mutations.
        var reactions = new List<BombBlockReaction>();
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            level,
            sharedProjectiles.PowerBombExplosion.Position,
            sharedProjectiles.PowerBombExplosion.ExplosionRadius,
            reactions,
            roomPlms,
            samus.LiquidPhysics.AreaIndex,
            slot.Type);
    }
}
