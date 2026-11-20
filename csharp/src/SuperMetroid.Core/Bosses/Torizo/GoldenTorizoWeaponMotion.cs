namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>How Golden Torizo's caught super missile and its eye beam move themselves (#1843, #2554).</summary>
internal enum GoldenTorizoWeaponMotion
{
    /// <summary>Keeps a held super missile at Golden Torizo's hand.</summary>
    SuperMissileHeld,

    /// <summary>Drops a thrown super missile under gravity until it strikes the room.</summary>
    SuperMissileThrown,

    /// <summary>Flies Golden Torizo's eye beam until it strikes the room.</summary>
    EyeBeamFlying,
}
