using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// The Ceres elevator shaft's main (#2287): its turn, which publishes a tilt for the following frame's presentation, and its
/// elevator, which may take Samus to the Ceres elevator only while main gameplay runs it.
/// </summary>
/// <param name="shaft">The shaft's turn and elevator.</param>
[RoomLifetime]
internal sealed class CeresElevatorShaftMain(CeresElevatorShaftRoomMainState shaft) : IRoomMain
{
    /// <summary>The shaft's turn and elevator, which the main steps and checks read.</summary>
    private readonly CeresElevatorShaftRoomMainState shaft = shaft;

    /// <inheritdoc/>
    public RoomMainOutcome Run(RoomMainUpdate update)
    {
        CeresElevatorShaftRoomMainResult result = shaft.Step(
            update.Player.Samus,
            update.Room.Enemies.Ceres.CeresStatus == CeresEscapeStatus.ElevatorDoorDestroyed,
            allowDeparture: update.AllowCeresElevatorDeparture);
        if (result.TiltChanged)
            update.Display.ActiveTurningFloor = result.Transform;
        return result.Outcome switch
        {
            CeresShaftOutcome.ElevatorTookSamus => RoomMainOutcome.CeresElevatorTookSamus,
            CeresShaftOutcome.Nothing => RoomMainOutcome.None,
            _ => throw new InvalidOperationException($"Undefined {nameof(CeresShaftOutcome)} {result.Outcome}."),
        };
    }
}
