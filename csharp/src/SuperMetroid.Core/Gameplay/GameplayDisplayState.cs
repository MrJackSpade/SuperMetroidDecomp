using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What the gameplay display currently shows: the elevator shaft's transform and the latched scene.
/// </summary>
internal sealed partial class GameplayDisplayState
{
    /// <summary>
    /// The Ceres elevator shaft's current turning floor. The shaft's door shows it as the room loads and the shaft room
    /// main publishes it after the drawing pass; sprite producers use it only while calculating their temporary rendered
    /// origins, and the next frame boundary shows it. The consumer never moves physical coordinates.
    /// </summary>
    public TurningFloorTransform? ActiveTurningFloor { get; set; }

    /// <summary>
    /// The scene the most recent shown (or loading) frame shows, which only the frame boundary latches (#2498). An update
    /// without a frame boundary leaves it unchanged; before any frame boundary it shows no room.
    /// </summary>
    public GameplayDisplayLatch Latched { get; internal set; } = LatchBeforeFirstRoom.NothingLatched;
}
