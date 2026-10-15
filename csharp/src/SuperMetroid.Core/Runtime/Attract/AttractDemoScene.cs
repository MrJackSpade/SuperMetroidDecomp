using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// One title-demo scene joined across the native room, equipment, and Samus setup tables.
/// The room loader increments the global scene index after placement; these records all
/// refer to the same pre-increment index, not the following scene's equipment.
/// </summary>
/// <remarks>
/// <c>CameraPosition</c> is the initial BG1 camera top-left; <c>SamusOffset</c> places Samus relative to
/// the camera, X from the screen's horizontal centre and Y from its top.
/// </remarks>
public sealed record AttractDemoScene(
    ushort RoomPointer, ushort DoorPointer, Position16<RoomPixels> CameraPosition, Offset16<RoomPixels> SamusOffset,
    ushort Duration, AttractDemoRoomSetup RoomSetupPointer, AttractDemoSamusSetup SamusSetupPointer,
    ushort Items, ushort Missiles, ushort SuperMissiles, ushort PowerBombs,
    ushort Health, ushort CollectedBeams, ushort EquippedBeams, ushort InputObject)
{
    /// <summary>Native wrapping placement: camera + half-screen horizontally + the signed offset on each axis.</summary>
    public Position16<RoomPixels> SamusPosition => CameraPosition + LoadStationEntry.SamusOffsetOrigin + SamusOffset;
}
