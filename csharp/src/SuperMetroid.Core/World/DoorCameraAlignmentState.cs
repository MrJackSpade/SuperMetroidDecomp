using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.World;

/// <summary>
/// One update's step of the camera toward its screen's edge while a door transition begins, in the room Samus leaves.
/// </summary>
/// <remarks>
/// Horizontal doors move the camera's Y toward the nearest screen boundary; vertical doors
/// move its X, one pixel per update: a camera past the middle of its screen moves on to the next screen's edge and one
/// before it back to its own. Completion is reported only by the following update that observes an already aligned
/// coordinate.
/// </remarks>
internal readonly partial record struct DoorCameraAlignmentState(
    Position16<RoomPixels> CameraPosition,
    bool Completed)
{
    /// <summary>Executes one exact coordinate step for the supplied door direction.</summary>
    public static DoorCameraAlignmentState Step(
        DoorDirection direction,
        Position16<RoomPixels> camera)
    {
        bool alignsX = RoomLoader.DoorTransitionAlignsX(direction);
        int coordinate = alignsX ? camera.X : camera.Y;
        int withinScreen = RoomGridConversions.OffsetWithinScreen(coordinate);
        if (withinScreen == 0)
            return new DoorCameraAlignmentState(camera, Completed: true);

        // A coordinate past the middle of its 256-pixel screen moves on to the next screen; one before it moves back.
        bool onToNextScreen = withinScreen >= DoorOpeningScrollDefinitions.CameraAlignmentNextScreenFrom;
        Offset16<RoomPixels> movement = alignsX
            ? new(1.Toward(onToNextScreen ? HorizontalDirection.Right : HorizontalDirection.Left), 0)
            : new(0, 1.Toward(onToNextScreen ? VerticalDirection.Down : VerticalDirection.Up));
        return new DoorCameraAlignmentState(camera + movement, Completed: false);
    }
}
