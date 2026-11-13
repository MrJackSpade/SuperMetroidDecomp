namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The ways a Ceres steam vent a room places can blow; the room's population names each vent's way.</summary>
public enum CeresSteamVariant
{
    /// <summary>Blows upward.</summary>
    Up,

    /// <summary>Blows leftward.</summary>
    Left,

    /// <summary>Blows downward.</summary>
    Down,

    /// <summary>Blows rightward.</summary>
    Right,

    /// <summary>Blows leftward, its picture offset for the rotating elevator room.</summary>
    RotatingElevatorLeft,

    /// <summary>Blows rightward, its picture offset for the rotating elevator room.</summary>
    RotatingElevatorRight,
}
