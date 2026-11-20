namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// How Crocomire's mouth shot moves itself (#1843): it aims once, then flies.
/// </summary>
internal enum CrocomireProjectileMotion
{
    /// <summary>Moves Crocomire's shot once, then derives its fired vector.</summary>
    ShotAiming,

    /// <summary>Flies Crocomire's shot until it strikes the room horizontally or vertically.</summary>
    ShotFlying,
}
