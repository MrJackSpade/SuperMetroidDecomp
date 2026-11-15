namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The main behaviour a steam vent's population variant gives it (#1870).
/// Its values are arbitrary.
/// </summary>
public enum CeresSteamAction
{
    /// <summary>Holds the ordinary vent where it is placed: its room position is also where it is drawn.</summary>
    HoldsItsPlace,
    /// <summary>Applies the rotating-elevator graphical transform.</summary>
    ApplyRotatingElevatorOffset,
}
