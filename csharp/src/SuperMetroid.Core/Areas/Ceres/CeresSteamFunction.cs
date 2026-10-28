namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The steam actor's mutually exclusive main behaviours, chosen by its population variant.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1415).
/// </summary>
public enum CeresSteamFunction
{
    /// <summary><c>$A6:EFF4</c>, the shared return: no graphical offset is required.</summary>
    None = 1,

    /// <summary><c>$A6:F019</c>: applies the rotating-elevator graphical transform.</summary>
    ApplyRotatingElevatorOffset,
}
