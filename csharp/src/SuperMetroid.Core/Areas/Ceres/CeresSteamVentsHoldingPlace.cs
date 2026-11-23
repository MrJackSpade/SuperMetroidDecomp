namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The motions of the steam vents of a room that is no turning elevator shaft (#2480): every vent there holds its place,
/// and a vent whose variant rides the shaft's turn cannot be placed.
/// </summary>
internal sealed partial class CeresSteamVentsHoldingPlace : CeresSteamVentMotions
{
    /// <summary>The one such room's motions.</summary>
    internal static CeresSteamVentsHoldingPlace Instance { get; } = new();

    /// <summary>Only <see cref="Instance"/> exists.</summary>
    private CeresSteamVentsHoldingPlace()
    {
    }

    /// <inheritdoc/>
    internal override CeresSteamMotion MotionOf(CeresSteamAction action) => action switch
    {
        CeresSteamAction.HoldsItsPlace => CeresSteamMotion.HoldsItsPlace.Instance,
        CeresSteamAction.ApplyRotatingElevatorOffset => throw new InvalidDataException(
            "A rotating-elevator Ceres steam vent is placed in a room that is no turning elevator shaft."),
        _ => throw new InvalidDataException($"Ceres steam has no behaviour for action {action}."),
    };
}
