namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres door's and wall's main behaviours.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1415).
/// </summary>
internal enum CeresDoorFunction
{
    /// <summary><c>$A6:F76B Function_CeresDoor_HandleEarthquakeDuringEscape</c>.</summary>
    HandleEarthquakeDuringEscape = 1,

    /// <summary><c>$A6:F770 Function_CeresDoor_HandleEarthquakeDuringEscapeInRidleysRoom</c>.</summary>
    HandleEarthquakeDuringEscapeInRidleysRoom,

    /// <summary><c>$A6:F7A5 Function_CeresDoor_RidleyEscapeMode7Wall</c>: the wall beside Ridley's rotating escape floor.</summary>
    RidleyEscapeFloorWall,

    /// <summary><c>$A6:F7BD Function_CeresDoor_RotatingElevatorRoom_Default</c>.</summary>
    RotatingElevatorRoomDefault,

    /// <summary><c>$A6:F7DC</c>: the rotating-elevator destruction rumble and explosions.</summary>
    RotatingElevatorRumble,

    /// <summary><c>$A6:F850</c>: the perpetual rotating-elevator palette animation.</summary>
    ElevatorAnimation,
}
