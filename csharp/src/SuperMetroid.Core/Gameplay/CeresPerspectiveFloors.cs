using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The perspective floors Ceres shows, made from the turns its rooms compute during the main loop (#1407).</summary>
internal static class CeresPerspectiveFloors
{
    /// <summary>
    /// The elevator shaft's floor: turned by the shaft's tilt at its own size about the shaft's centre, and scrolled with
    /// layer 1.
    /// </summary>
    /// <param name="shaft">The tilt the shaft's room main last set.</param>
    /// <param name="foregroundScroll">Layer 1's scroll in the frame that shows the floor.</param>
    internal static PerspectiveFloor ElevatorShaft(TurningFloorTransform shaft, Position16<RoomPixels> foregroundScroll) => new(
        new FloorView(shaft.Angle, FloorView.OwnSize, shaft.Center.AsFloorWord(), foregroundScroll.AsFloorWord()),
        BelowPerspectiveFloor.Nothing);

    /// <summary>The floor Ceres Ridley's getaway flies over, with the room's layer 2 below it.</summary>
    /// <param name="ridley">How the getaway's floor appears as its room main last turned and moved it (<see cref="CeresRidleyGetaway.FloorTransform"/>).</param>
    internal static PerspectiveFloor RidleyGetaway(FloorView ridley) => new(ridley, BelowPerspectiveFloor.RoomBackground);
}
