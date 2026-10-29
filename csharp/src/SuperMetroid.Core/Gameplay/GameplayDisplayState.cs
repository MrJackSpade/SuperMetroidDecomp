using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What the gameplay display currently shows: the elevator shaft's transform, the latched scene and the default layer
/// blending.
/// </summary>
internal sealed class GameplayDisplayState
{

    /// <summary>
    /// Native WRAM <c>$1982</c>. Room/HDMA setup owns this word; the current Landing Site
    /// shell starts at bank-$88's cleared-FX default two.
    /// </summary>
    public LayerBlendingConfiguration LayerBlendingDefaultConfig { get; set; } =
        LayerBlendingConfiguration.NormalGameplay;

    /// <summary>
    /// The Ceres elevator shaft's current turning floor. The shaft room main publishes it after the
    /// drawing pass, and sprite producers use it only while calculating their temporary
    /// rendered origins. The translated consumer never moves physical coordinates.
    /// </summary>
    public TurningFloorTransform? ActiveTurningFloor { get; set; }

    /// <summary>
    /// The scene the most recent accepted (or forced-blank) frame shows. A lag frame leaves it unchanged. Door setup
    /// that shows the elevator shaft's floor before the room becomes visible updates it through
    /// <see cref="PublishDoorElevatorFloor"/>.
    /// </summary>
    public GameplayDisplayLatch Latched { get; internal set; } = InitialLatch;


    /// <summary>
    /// Door setup shows the elevator shaft's floor before the room is exposed (<c>$8F:E4E0</c>), or removes it
    /// (<c>$8F:E513</c>), scrolled with the latched layer-1 scroll. A getaway floor already latched stays, as it takes
    /// precedence over the shaft's. Later room-main changes reach the display only through the frame latch.
    /// </summary>
    /// <param name="transform">The shaft's transform, or null to remove the floor.</param>
    internal void PublishDoorElevatorFloor(TurningFloorTransform? transform) =>
        Latched = Latched with
        {
            PerspectiveFloor = Latched.PerspectiveFloor is { Below: BelowPerspectiveFloor.Layer2 } getaway ? getaway
                : transform is { } shaft ? CeresPerspectiveFloors.ElevatorShaft(shaft, Latched.Scrolls.Bg1Scroll)
                : null,
        };

    /// <summary>The latch in effect before any accepted frame: nothing scrolled, no floor and no active effects.</summary>
    private static GameplayDisplayLatch InitialLatch => new(
        default, default, null, null, null, null, 0,
        new GameplayLatchedScene(default,
            LayerBlendingConfiguration.Unconfigured, null, null, 0),
        default, default, null, null);
}
