using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Where loading at a load station puts Samus: loading enters <see cref="Room"/> through <see cref="Door"/> with the
/// camera and Samus placed as the station states.
/// </summary>
/// <param name="Room">The room the station loads.</param>
/// <param name="Door">The door through which the room is entered.</param>
/// <param name="CameraPosition">The initial camera top-left.</param>
/// <param name="SamusOffset">Samus's placement relative to the camera, X from the screen's horizontal centre and Y from the camera top.</param>
public sealed record LoadStationPlacement(
    RoomDefinition Room,
    DoorDefinition Door,
    Position16<RoomPixels> CameraPosition,
    Offset16<RoomPixels> SamusOffset)
{
    /// <summary>The camera-relative origin of <see cref="SamusOffset"/>: half a screen right of the camera.</summary>
    public static readonly Offset16<RoomPixels> SamusOffsetOrigin = new(128, 0);

    /// <summary>The room position loading at the station places Samus at, wrapping each axis at sixteen bits.</summary>
    public Position16<RoomPixels> SamusPosition => CameraPosition + SamusOffsetOrigin + SamusOffset;
}
