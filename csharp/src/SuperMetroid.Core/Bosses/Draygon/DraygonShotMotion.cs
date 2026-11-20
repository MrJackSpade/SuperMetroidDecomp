namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// How Draygon's goop or a wall turret's shot moves itself in flight (#1843); a goop stuck to Samus follows her.
/// </summary>
internal enum DraygonShotMotion
{
    /// <summary>Flies Draygon's goop, sticks it to Samus when she is near, and removes it outside the room.</summary>
    GoopFlying,

    /// <summary>Flies a wall turret's aimed shot at full precision until it leaves the room.</summary>
    TurretShotFlying,
}
