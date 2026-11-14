using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// One title-demo scene joined across the room, equipment, and Samus setup definitions: the room, staging and
/// equipment of one demonstration of an <see cref="AttractDemoSet"/>.
/// </summary>
/// <remarks>
/// <c>Duration</c> is the number of demo frames the scene plays before the attract sequence moves on.
/// <paramref name="CameraPosition"/> is the initial camera top-left; <paramref name="SamusOffset"/> places Samus relative to
/// the camera, X from the screen's horizontal centre and Y from its top.
/// </remarks>
public sealed record AttractDemoScene(
    RoomDefinition Room, DoorDefinition Door, Position16<RoomPixels> CameraPosition, Offset16<RoomPixels> SamusOffset,
    int Duration, AttractDemoRoomSetup RoomSetup, AttractDemoSamusSetup SamusSetup,
    SuperMetroid.Core.Progression.SamusEquipmentFlags Items, int Missiles, int SuperMissiles, int PowerBombs,
    int Health, SuperMetroid.Core.Progression.SamusBeamFlags CollectedBeams, SuperMetroid.Core.Progression.SamusBeamFlags EquippedBeams, AttractDemoInputDefinition Input)
{
    /// <summary>Wrapping placement: camera + half-screen horizontally + the signed offset on each axis.</summary>
    public Position16<RoomPixels> SamusPosition => CameraPosition + LoadStationPlacement.SamusOffsetOrigin + SamusOffset;
}
