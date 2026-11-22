using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The motions of the turning elevator shaft's steam vents (#2480): built only for the room whose scope holds the
/// shaft's turn, which its rotating-elevator vents ride; any other vent there holds its place.
/// </summary>
[RoomLifetime]
internal sealed partial class CeresShaftSteamVentMotions : CeresSteamVentMotions
{
    /// <summary>Creates a <see cref="CeresShaftSteamVentMotions"/>.</summary>
    /// <param name="shaft">The shaft's turn, which the shaft's elevator door starts as the room loads.</param>
    public CeresShaftSteamVentMotions(CeresElevatorShaftRoomMainState shaft)
    {
        this._ridesTurn = new(Ensure.NotNull(shaft));
    }

    /// <summary>The motion every rotating-elevator vent of the shaft shares: riding the shaft's turn.</summary>
    private readonly CeresSteamMotion.RidesShaftTurn _ridesTurn;

    /// <inheritdoc/>
    internal override CeresSteamMotion MotionOf(CeresSteamAction action) => action switch
    {
        CeresSteamAction.HoldsItsPlace => CeresSteamMotion.HoldsItsPlace.Instance,
        CeresSteamAction.ApplyRotatingElevatorOffset => _ridesTurn,
        _ => throw new InvalidDataException($"Ceres steam has no behaviour for action {action}."),
    };
}
