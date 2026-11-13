namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The steam actor's mutually exclusive main behaviours, chosen by its population variant.
/// Its values are arbitrary.
/// </summary>
public enum CeresSteamAction
{
    /// <summary>The shared return: no graphical offset is required.</summary>
    None = 1,

    /// <summary>Applies the rotating-elevator graphical transform.</summary>
    ApplyRotatingElevatorOffset,
}
