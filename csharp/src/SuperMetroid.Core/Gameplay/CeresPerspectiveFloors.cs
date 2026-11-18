using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The perspective floors Ceres shows, made from the turns its rooms compute during the update (#1407).</summary>
internal static class CeresPerspectiveFloors
{
    /// <summary>
    /// The elevator shaft's floor: turned by the shaft's tilt at its own size about the shaft's centre, scrolled with the
    /// foreground, and repeating beyond its edges, so a tilted shaft shows the far wall's columns where the floor runs out
    /// (#1653); Ceres Ridley's getaway floor shows nothing beyond its edges. Nothing shows below it: it fills the gameplay view.
    /// </summary>
    /// <param name="shaft">The tilt the shaft's room main last set.</param>
    /// <param name="foregroundScroll">The foreground's scroll in the frame that shows the floor.</param>
    internal static PerspectiveFloor ElevatorShaft(TurningFloorTransform shaft, Offset16<RoomPixels> foregroundScroll) => new(
        new FloorView(shaft.Angle, FloorView.OwnSize, shaft.Center.AsFloorOffset(), foregroundScroll.AsFloorOffset(), FloorOverflowMode.Wrap));

    /// <summary>
    /// The floor Ceres Ridley's getaway flies over, with the chamber's walls around it (#1545); the room's background shows in
    /// a band below it.
    /// </summary>
    /// <param name="ridley">How the getaway's floor appears as its room main last turned and moved it (<see cref="CeresEscape.ShownFloor"/>).</param>
    /// <param name="chamberWalls">The chamber's walls, which stand behind the floor (<see cref="CeresEscape.ChamberWalls"/>).</param>
    internal static PerspectiveFloor RidleyGetaway(FloorView ridley, ImmutableArray<PerspectiveFloorScenery> chamberWalls) =>
        new(ridley, chamberWalls);
}
