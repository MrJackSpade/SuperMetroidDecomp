using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// One title-demo scene joined across the native room, equipment, and Samus setup tables.
/// The room loader increments the global scene index after placement; these records all
/// refer to the same pre-increment index, not the following scene's equipment.
/// </summary>
/// <remarks>
/// <c>Duration</c> is the number of demo frames the scene plays before the attract sequence moves on.
/// <c>CameraPosition</c> is the initial BG1 camera top-left; <c>SamusOffset</c> places Samus relative to
/// the camera, X from the screen's horizontal centre and Y from its top.
/// </remarks>
public sealed record AttractDemoScene(
    RoomHeaderDefinition Room, CartridgeDoorHeader Door, Position16<RoomPixels> CameraPosition, Offset16<RoomPixels> SamusOffset,
    int Duration, AttractDemoRoomSetup RoomSetupPointer, AttractDemoSamusSetup SamusSetupPointer,
    SuperMetroid.Core.Samus.SamusEquipmentFlags Items, int Missiles, int SuperMissiles, int PowerBombs,
    int Health, SuperMetroid.Core.Samus.Projectiles.SamusBeamFlags CollectedBeams, SuperMetroid.Core.Samus.Projectiles.SamusBeamFlags EquippedBeams, SuperMetroid.Core.Input.DemoInputObjectDefinition InputObject)
{
    /// <summary>Native wrapping placement: camera + half-screen horizontally + the signed offset on each axis.</summary>
    public Position16<RoomPixels> SamusPosition => CameraPosition + LoadStationEntry.SamusOffsetOrigin + SamusOffset;
}
