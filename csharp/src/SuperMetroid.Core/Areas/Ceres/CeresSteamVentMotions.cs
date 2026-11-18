namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The motions a room gives the steam vents it places (#2480), by the action each vent's variant names: the turning
/// elevator shaft's (<see cref="CeresShaftSteamVentMotions"/>), built only for the room whose scope holds the shaft's
/// turn, or every other room's (<see cref="CeresSteamVentsHoldingPlace"/>).
/// </summary>
internal abstract class CeresSteamVentMotions
{
    /// <summary>The motion <paramref name="action"/> gives a vent the room places.</summary>
    /// <exception cref="InvalidDataException">The room cannot place a vent of that action, or the action is undefined.</exception>
    internal abstract CeresSteamMotion MotionOf(CeresSteamAction action);
}
