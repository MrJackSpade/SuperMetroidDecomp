namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive population variants accepted by the Ceres steam initializer.
/// The numeric values are the exact parameter-one indexes used by <c>$A6:EFB1</c>.
/// </summary>
public enum CeresSteamVariant : ushort
{
    /// <summary>Parameter one value zero, selecting the upward steam instruction program.</summary>
    Up = 0,

    /// <summary>Parameter one value one, selecting the leftward steam instruction program.</summary>
    Left = 1,

    /// <summary>Parameter one value two, selecting the downward steam instruction program.</summary>
    Down = 2,

    /// <summary>Parameter one value three, selecting the rightward steam instruction program.</summary>
    Right = 3,

    /// <summary>Parameter one value four, reusing leftward animation with the rotating-elevator graphical offset.</summary>
    RotatingElevatorLeft = 4,

    /// <summary>Parameter one value five, reusing rightward animation with the rotating-elevator graphical offset.</summary>
    RotatingElevatorRight = 5,
}
