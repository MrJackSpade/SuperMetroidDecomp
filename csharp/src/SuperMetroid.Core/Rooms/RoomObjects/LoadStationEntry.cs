using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One load station's entry, which loading the game at that station reads.</summary>
/// <param name="Room">The room, or null for an unused slot.</param>
/// <param name="Door">The door through which the room is entered, or null for an unused slot.</param>
/// <param name="CameraPosition">Initial camera top-left (entry words 6 and 8).</param>
/// <param name="SamusOffset">
/// Samus's placement relative to the camera (entry words 12 and 10): X is measured from the
/// screen's horizontal centre, Y from the camera top.
/// </param>
public sealed record LoadStationEntry(
    RoomHeaderDefinition? Room,
    DoorDefinition? Door,
    Position16<RoomPixels> CameraPosition,
    Offset16<RoomPixels> SamusOffset)
{
    /// <summary>The camera-relative origin of <see cref="SamusOffset"/>: half a screen right of the camera.</summary>
    public static readonly Offset16<RoomPixels> SamusOffsetOrigin = new(128, 0);

    /// <summary>World position assigned by the load-station routine, wrapping each axis at sixteen bits.</summary>
    public Position16<RoomPixels> SamusPosition => CameraPosition + SamusOffsetOrigin + SamusOffset;
}
