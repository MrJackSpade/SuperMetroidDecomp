namespace SuperMetroid.Core.Runtime;

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
    ushort CameraX,
    ushort CameraY,
    bool Completed)
{
    /// <summary>Executes one exact coordinate step for the supplied bank-$83 orientation.</summary>
    public static DoorCameraAlignmentState Step(
        byte orientation,
        ushort cameraX,
        ushort cameraY)
    {
        bool alignsX = SuperMetroidRuntime.DoorTransitionAlignsX(orientation);
        ushort coordinate = alignsX ? cameraX : cameraY;
        byte lowByte = unchecked((byte)coordinate);
        if (lowByte == 0)
            return new DoorCameraAlignmentState(cameraX, cameraY, Completed: true);

        coordinate = (lowByte & 0x80) != 0
            ? unchecked((ushort)(coordinate + 1))
            : unchecked((ushort)(coordinate - 1));
        return alignsX
            ? new DoorCameraAlignmentState(coordinate, cameraY, Completed: false)
            : new DoorCameraAlignmentState(cameraX, coordinate, Completed: false);
    }
}
