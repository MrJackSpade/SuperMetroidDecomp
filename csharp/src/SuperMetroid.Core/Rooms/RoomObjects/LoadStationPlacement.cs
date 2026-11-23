using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Where loading at a load station puts Samus (#2483): loading enters the room <see cref="Door"/> leads to through it, with
/// the camera and Samus placed as the station states.
/// </summary>
/// <param name="Door">The door through which the station's room is entered.</param>
/// <param name="CameraPosition">The initial camera top-left.</param>
/// <param name="SamusPosition">The room position loading at the station places Samus at.</param>
public sealed partial record LoadStationPlacement(
    DoorDefinition Door,
    Position16<RoomPixels> CameraPosition,
    Position16<RoomPixels> SamusPosition);
