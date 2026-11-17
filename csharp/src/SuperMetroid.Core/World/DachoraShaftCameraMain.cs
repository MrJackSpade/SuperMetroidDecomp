using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Movement;
using static SuperMetroid.Core.Areas.Brinstar.DachoraShaftCameraDefinitions;

namespace SuperMetroid.Core.World;

/// <summary>
/// The Dachora room's main (#2293): while the shaft's top screen stands open, a camera above and left of the shaft's
/// bounds moves right a few pixels each update, frozen time or not; Samus's next tracking starts from where it leaves the
/// camera. A shaft screen open with the camera aligned to its top does not move it.
/// </summary>
internal sealed class DachoraShaftCameraMain : IRoomMain
{
    /// <inheritdoc/>
    public void Run(RoomMainUpdate update) => NudgeTowardShaft(Ensure.NotNull(update).Room.Camera);

    /// <summary>Moves <paramref name="camera"/> toward the shaft once, when the shaft is open and the camera within its bounds.</summary>
    /// <param name="camera">The room's camera.</param>
    private static void NudgeTowardShaft(ScrollBoundaryCamera camera)
    {
        if (camera.Scrolls.ReadScreen(ShaftTopScreen) != RoomScrollState.Open)
            return;
        Position16<RoomPixels> position = camera.PixelPosition;
        if (position.Y >= CameraAboveY || position.X >= CameraLeftOfX)
            return;
        camera.NudgeRight(StepPixels);
    }
}
