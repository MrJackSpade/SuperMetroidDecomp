namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One fourteen-byte entry consumed by <c>LoadFromLoadStation</c> at $80:C437.</summary>
/// <param name="Room">The room, or null for an unused slot.</param>
/// <param name="DoorPointer">Bank-$83 door through which the room is entered.</param>
/// <param name="CameraPosition">Initial BG1 camera top-left (entry words 6 and 8).</param>
/// <param name="SamusOffset">
/// Samus's placement relative to the camera (entry words 12 and 10): X is measured from the
/// screen's horizontal centre, Y from the camera top.
/// </param>
public sealed record LoadStationEntry(
    RoomHeaderDefinition? Room,
    ushort DoorPointer,
    Position16<RoomPixels> CameraPosition,
    Offset16<RoomPixels> SamusOffset)
{
    /// <summary>The camera-relative origin of <see cref="SamusOffset"/>: half a screen right of the camera.</summary>
    public static readonly Offset16<RoomPixels> SamusOffsetOrigin = new(128, 0);

    /// <summary>World position assigned by the native load-station routine, wrapping each axis at sixteen bits.</summary>
    public Position16<RoomPixels> SamusPosition => CameraPosition + SamusOffsetOrigin + SamusOffset;
}
