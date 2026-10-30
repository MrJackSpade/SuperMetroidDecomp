using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Chainsaw callback (<c>$90:B0AC</c>), reached by uncharged beam combination thirteen: it runs the Power Bomb
/// pre-instruction over the shot. Its misaligned store into the cached window registers is a presentation quirk the
/// port drops (#1407).
/// </summary>
internal sealed class ChainsawPowerBombCallback
{
    /// <summary>Projectile animation owner used when the callback clears its slot.</summary>
    private readonly ProjectileAnimation _animation;

    /// <summary>Connects the callback to projectile animation cleanup.</summary>
    internal ChainsawPowerBombCallback(ProjectileAnimation animation)
    {
        _animation = animation;
    }

    /// <summary>Runs the Power Bomb fuse over the shot, then the bank-$94 boundary dispatcher.</summary>
    internal void RunPowerBombPreInstruction(
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSlot slot,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomPlms? roomPlms,
        InstalledSamusOverreads? overreads)
    {
        InstalledProjectileListPositions Lists() => (overreads ?? throw new InvalidOperationException(
            "The chainsaw window store needs the installed Samus overread catalog.")).ProjectileLists;
        // The misaligned callback runs the Power Bomb fuse over the shot, from a zero fuse.
        PowerBombFuseStep fuse = SamusPowerBombFuse.Step(slot.ChainsawPowerBombFuse, sharedProjectiles.PowerBombExplosion.IsArmed);
        slot.ChainsawPowerBombFuse = fuse.Timer;
        // On a beam the fuse's fast-list switch adds its offset to the beam's own list pointer, so where it lands is
        // the native list layout's: a glitch the port reproduces through that layout.
        if (fuse.FastAnimation && slot.Cursor is { } live)
            slot.Cursor = Lists().Offset(live, SamusPowerBombFuseData.FastAnimationOffset);

        if (fuse.SpawnExplosion)
            sharedProjectiles.PowerBombExplosion.Spawn(slot.Position.Whole);
        if (fuse.DeleteProjectile)
            _animation.ClearProjectile(slot);

        // `$90:B0AE` always falls through from PowerBomb_Func3 to `$94:9CAC`, even
        // when the zero-timer/no-flag branch just cleared the projectile. The latter
        // case consequently dispatches as no projectile, while both paths
        // use the global Power Bomb centre/radius rather than the Chainsaw's position.
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
