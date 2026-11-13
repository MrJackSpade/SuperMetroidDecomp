namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres door's and wall's main behaviours.
/// Its values are arbitrary.
/// </summary>
internal enum CeresDoorAction
{
    /// <summary>Shakes the screen while Ceres self-destructs.</summary>
    HandleEarthquakeDuringEscape = 1,

    /// <summary>Shakes the screen while Ceres self-destructs, in Ridley's room.</summary>
    HandleEarthquakeDuringEscapeInRidleysRoom,

    /// <summary>The wall beside Ridley's rotating escape floor.</summary>
    RidleyEscapeFloorWall,

    /// <summary>The rotating elevator room's default behaviour, before the destruction rumble.</summary>
    RotatingElevatorRoomDefault,

    /// <summary>The rotating-elevator destruction rumble and explosions.</summary>
    RotatingElevatorRumble,

    /// <summary>The perpetual rotating-elevator palette animation.</summary>
    ElevatorAnimation,
}
