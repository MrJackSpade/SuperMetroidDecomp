namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A6 Ceres-door main functions stored in the actor's variable A ($0FA8).</summary>
internal enum CeresDoorFunction : ushort
{
    /// <summary><c>Function_CeresDoor_HandleEarthquakeDuringEscape</c> at $A6:F76B.</summary>
    HandleEarthquakeDuringEscape = 0xf76b,

    /// <summary><c>Function_CeresDoor_HandleEarthquakeDuringEscapeInRidleysRoom</c> at $A6:F770.</summary>
    HandleEarthquakeDuringEscapeInRidleysRoom = 0xf770,

    /// <summary><c>Function_CeresDoor_RidleyEscapeMode7Wall</c> at $A6:F7A5.</summary>
    RidleyEscapeMode7Wall = 0xf7a5,

    /// <summary><c>Function_CeresDoor_RotatingElevatorRoom_Default</c> at $A6:F7BD.</summary>
    RotatingElevatorRoomDefault = 0xf7bd,

    /// <summary>The rotating-elevator destruction rumble and explosions at $A6:F7DC.</summary>
    RotatingElevatorRumble = 0xf7dc,

    /// <summary>The perpetual rotating-elevator palette animation at $A6:F850.</summary>
    ElevatorAnimation = 0xf850,
}
