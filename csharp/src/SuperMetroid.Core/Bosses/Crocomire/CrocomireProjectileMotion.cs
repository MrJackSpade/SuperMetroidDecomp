namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// How one of Crocomire's projectiles moves itself (#1843): his mouth shot, his bridge's fragments and the spike wall's pieces.
/// </summary>
internal enum CrocomireProjectileMotion
{
    /// <summary>Moves Crocomire's shot once, then derives its fired vector.</summary>
    ShotAiming,

    /// <summary>Flies Crocomire's shot until it strikes the room horizontally or vertically.</summary>
    ShotFlying,

    /// <summary>Accelerates and drops each spike-wall piece by its own rule.</summary>
    SpikeWallCollapsing,

    /// <summary>Drops a crumbling bridge fragment under gravity until it strikes the room.</summary>
    BridgeFragmentFalling,
}
