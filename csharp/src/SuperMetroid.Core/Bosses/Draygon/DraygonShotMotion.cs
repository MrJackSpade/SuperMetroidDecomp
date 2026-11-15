namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// How Draygon's goop or a wall turret's shot moves itself (#1843).
/// </summary>
internal enum DraygonShotMotion
{
    /// <summary>Flies Draygon's goop, sticks it to Samus when she is near, and removes it outside the room.</summary>
    GoopFlying,

    /// <summary>Keeps Draygon's goop stuck to Samus for its lifetime.</summary>
    GoopStuckToSamus,

    /// <summary>Flies a wall turret's aimed shot at full precision until it leaves the room.</summary>
    TurretShotFlying,
}
