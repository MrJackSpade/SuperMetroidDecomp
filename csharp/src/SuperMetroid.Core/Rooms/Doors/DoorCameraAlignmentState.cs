namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One invocation of the cartridge's source-camera alignment coroutine at
/// <c>$82:E310-$82:E352</c>.
/// </summary>
/// <remarks>
/// Horizontal doors align the camera's vertical low byte; vertical doors align its
/// horizontal low byte. The signed-byte branch deliberately approaches zero through
/// either <c>$01 -> $00</c> or <c>$FF -> $00</c>. Completion is reported only by the
/// following call that observes an already aligned coordinate.
/// </remarks>
internal readonly record struct DoorCameraAlignmentState(
    Position16<RoomPixels> CameraPosition,
    bool Completed)
{
    /// <summary>Executes one exact coordinate step for the supplied bank-$83 door direction.</summary>
    public static DoorCameraAlignmentState Step(
        DoorDirection direction,
        Position16<RoomPixels> camera)
    {
        bool alignsX = RoomLoader.DoorTransitionAlignsX(direction);
        ushort coordinate = alignsX ? camera.X : camera.Y;
        byte lowByte = unchecked((byte)coordinate);
        if (lowByte == 0)
            return new DoorCameraAlignmentState(camera, Completed: true);

        short step = (lowByte & 0x80) != 0 ? (short)1 : (short)-1;
        Offset16<RoomPixels> movement = alignsX ? new(step, 0) : new(0, step);
        return new DoorCameraAlignmentState(camera + movement, Completed: false);
    }
}
