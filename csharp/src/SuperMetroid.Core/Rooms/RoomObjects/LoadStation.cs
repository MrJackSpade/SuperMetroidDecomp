using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A place the game loads Samus at: a save station, an elevator's arrival, the Ceres start or the arrival on Zebes. A
/// save names the station it resumes at; loading enters <see cref="Room"/> through <see cref="Door"/> with the camera and
/// Samus placed as the station states.
/// </summary>
public sealed class LoadStation
{
    /// <summary>The camera-relative origin of <see cref="SamusOffset"/>: half a screen right of the camera.</summary>
    public static readonly Offset16<RoomPixels> SamusOffsetOrigin = new(128, 0);

    /// <summary>Names one load station.</summary>
    /// <param name="name">The station's name.</param>
    /// <param name="area">The area whose station list holds the station.</param>
    /// <param name="number">
    /// The station's number in its area's station list, which the save file, the map's used-station record and the save
    /// station's room object share.
    /// </param>
    /// <param name="room">The room the station loads.</param>
    /// <param name="door">The door through which the room is entered.</param>
    /// <param name="cameraPosition">The initial camera top-left.</param>
    /// <param name="samusOffset">Samus's placement relative to the camera, X from the screen's horizontal centre and Y from the camera top.</param>
    internal LoadStation(string name, AreaId area, int number, RoomHeaderDefinition room, DoorDefinition door,
        Position16<RoomPixels> cameraPosition, Offset16<RoomPixels> samusOffset)
    {
        Name = Ensure.NotNull(name);
        Area = area;
        Number = Ensure.AtLeastZero(number);
        Room = Ensure.NotNull(room);
        Door = Ensure.NotNull(door);
        CameraPosition = cameraPosition;
        SamusOffset = samusOffset;
    }

    /// <summary>The station's name.</summary>
    public string Name { get; }

    /// <summary>The area whose station list holds the station.</summary>
    public AreaId Area { get; }

    /// <summary>
    /// The station's number in its area's station list, which the save file, the map's used-station record and the save
    /// station's room object share.
    /// </summary>
    public int Number { get; }

    /// <summary>The room the station loads.</summary>
    public RoomHeaderDefinition Room { get; }

    /// <summary>The door through which the room is entered.</summary>
    public DoorDefinition Door { get; }

    /// <summary>The initial camera top-left.</summary>
    public Position16<RoomPixels> CameraPosition { get; }

    /// <summary>Samus's placement relative to the camera: X from the screen's horizontal centre, Y from the camera top.</summary>
    public Offset16<RoomPixels> SamusOffset { get; }

    /// <summary>The room position loading at the station places Samus at, wrapping each axis at sixteen bits.</summary>
    public Position16<RoomPixels> SamusPosition => CameraPosition + SamusOffsetOrigin + SamusOffset;

    /// <summary>The station's name.</summary>
    public override string ToString() => Name;
}
