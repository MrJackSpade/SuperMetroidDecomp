namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>How one of the Torizos' Chozo orbs moves itself (#1843, #2554).</summary>
internal enum TorizoChozoOrbMotion
{
    /// <summary>Drops a Bomb Torizo orb under gravity until it strikes the room.</summary>
    BombOrbFalling,

    /// <summary>Bounces a Golden Torizo orb off walls and, damped, off the floor.</summary>
    GoldenOrbBouncing,
}
