using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What the gameplay display currently shows: the elevator shaft's transform and the latched scene.
/// </summary>
internal sealed class GameplayDisplayState
{
    /// <summary>
    /// The Ceres elevator shaft's current turning floor. The shaft room main publishes it after the
    /// drawing pass, and sprite producers use it only while calculating their temporary
    /// rendered origins. The consumer never moves physical coordinates.
    /// </summary>
    public TurningFloorTransform? ActiveTurningFloor { get; set; }

    /// <summary>
    /// The scene the most recent shown (or loading) frame shows. An update without a frame boundary leaves it unchanged. Door setup
    /// that shows the elevator shaft's floor before the room becomes visible updates it through
    /// <see cref="PublishDoorElevatorFloor"/>.
    /// </summary>
    public GameplayDisplayLatch Latched { get; internal set; } = InitialLatch;


    /// <summary>
    /// Door setup shows the elevator shaft's floor before the room is exposed, or removes it,
    /// scrolled with the latched foreground scroll. A getaway floor already latched stays, as it takes
    /// precedence over the shaft's. Later room-main changes reach the display only through the frame latch.
    /// </summary>
    /// <param name="transform">The shaft's transform, or null to remove the floor.</param>
    internal void PublishDoorElevatorFloor(TurningFloorTransform? transform) =>
        Latched = Latched with
        {
            PerspectiveFloor = Latched.PerspectiveFloor is { Below: BelowPerspectiveFloor.RoomBackground } getaway ? getaway
                : transform is { } shaft ? CeresPerspectiveFloors.ElevatorShaft(shaft, Latched.Scrolls.ForegroundScroll)
                : null,
        };

    /// <summary>The latch in effect before any accepted frame: nothing scrolled, no floor and no active effects.</summary>
    private static GameplayDisplayLatch InitialLatch => new(
        default, default, null, null, null, null, 0,
        new GameplayLatchedScene(default,
            null, null, null, null),
        default, default, null, null);
}
