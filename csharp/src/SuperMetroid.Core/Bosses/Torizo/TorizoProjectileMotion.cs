namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How one of Bomb or Golden Torizo's projectiles moves itself (#1843).
/// </summary>
internal enum TorizoProjectileMotion
{
    /// <summary>Drops a Bomb Torizo statue fragment until it reaches the room's floor.</summary>
    StatueFragmentFalling,

    /// <summary>Drops a Bomb Torizo orb under gravity until it strikes the room.</summary>
    BombOrbFalling,

    /// <summary>Bounces a Golden Torizo orb off walls and, damped, off the floor.</summary>
    GoldenOrbBouncing,

    /// <summary>Flies a Torizo sonic boom horizontally, accelerating, until it strikes the room.</summary>
    SonicBoomFlying,

    /// <summary>Drags and drops Bomb Torizo's drool under gravity, splashing on impact.</summary>
    DroolFalling,

    /// <summary>Bounces a Golden Torizo egg for its timer, then launches it horizontally.</summary>
    EggBouncing,

    /// <summary>Accelerates a hatched Golden Torizo egg toward a wall.</summary>
    EggCharging,

    /// <summary>Drops a Golden Torizo egg that struck a wall to the floor, where it hatches or bursts.</summary>
    EggDropping,

    /// <summary>Keeps a held super missile at Golden Torizo's hand.</summary>
    SuperMissileHeld,

    /// <summary>Drops a thrown super missile under gravity until it strikes the room.</summary>
    SuperMissileThrown,

    /// <summary>Flies Golden Torizo's eye beam until it strikes the room.</summary>
    EyeBeamFlying,
}
