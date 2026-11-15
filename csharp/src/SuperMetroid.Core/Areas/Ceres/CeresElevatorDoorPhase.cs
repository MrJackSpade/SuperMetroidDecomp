namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Where the elevator room's blocked door stands in its destruction (#1953). Its values are arbitrary.
/// </summary>
internal enum CeresElevatorDoorPhase
{
    /// <summary>Still standing, until the station starts to self-destruct; it keeps the room's glow and platform turning.</summary>
    Intact,

    /// <summary>Blowing apart with a rumble of explosions.</summary>
    Exploding,

    /// <summary>Gone; it still keeps the room's glow and platform turning.</summary>
    Destroyed,
}
