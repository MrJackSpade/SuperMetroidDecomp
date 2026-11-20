namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How one of the Torizos' drool, sonic booms or statue fragments moves itself (#1843, #2554).
/// </summary>
internal enum TorizoProjectileMotion
{
    /// <summary>Drops a Bomb Torizo statue fragment until it reaches the room's floor.</summary>
    StatueFragmentFalling,

    /// <summary>Flies a Torizo sonic boom horizontally, accelerating, until it strikes the room.</summary>
    SonicBoomFlying,

    /// <summary>Drags and drops Bomb Torizo's drool under gravity, splashing on impact.</summary>
    DroolFalling,
}
