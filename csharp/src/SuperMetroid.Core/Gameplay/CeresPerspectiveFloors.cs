using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The perspective floors Ceres shows, made from the transforms its rooms compute during the main loop (#1407).</summary>
internal static class CeresPerspectiveFloors
{
    /// <summary>
    /// The elevator shaft's floor: the shaft's transform, whose two diagonal terms are both its first coefficient,
    /// turning about the shaft's centre and scrolled with layer 1.
    /// </summary>
    /// <param name="shaft">The transform the shaft's room main last computed.</param>
    /// <param name="layer1Scroll">Layer 1's scroll in the frame that shows the floor.</param>
    internal static PerspectiveFloor ElevatorShaft(SamusMode7Transform shaft, Position16<RoomPixels> layer1Scroll) => new(
        new AffineTransform(shaft.MatrixA, shaft.MatrixB, shaft.MatrixC, shaft.MatrixA, shaft.Center.AsFloorWord(),
            new Offset16<FloorPixels>(unchecked((short)layer1Scroll.X), unchecked((short)layer1Scroll.Y))),
        BelowPerspectiveFloor.Nothing);

    /// <summary>The floor Ceres Ridley's getaway flies over, with the room's layer 2 below it.</summary>
    /// <param name="ridley">The getaway's transform as its room main last wrote it (<see cref="CeresRidleyGetaway.FloorTransform"/>).</param>
    internal static PerspectiveFloor RidleyGetaway(AffineTransform ridley) => new(ridley, BelowPerspectiveFloor.Layer2);
}
