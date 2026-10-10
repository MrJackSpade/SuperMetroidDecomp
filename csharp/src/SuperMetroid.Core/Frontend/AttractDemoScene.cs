using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One title-demo scene joined across the native room, equipment, and Samus setup tables.
/// The room loader increments the global scene index after placement; these records all
/// refer to the same pre-increment index, not the following scene's equipment.
/// </summary>
public sealed record AttractDemoScene(
    ushort RoomPointer, ushort DoorPointer,     ushort CameraX, ushort CameraY, ushort SamusYFromTop, short SamusXFromCenter,
    ushort Duration, AttractDemoRoomSetup RoomSetupPointer, AttractDemoSamusSetup SamusSetupPointer,
    ushort Items, ushort Missiles, ushort SuperMissiles, ushort PowerBombs,
    ushort Health, ushort CollectedBeams, ushort EquippedBeams, ushort InputObject)
{
    /// <summary>Native wrapping X placement: camera + half-screen + signed offset.</summary>
    public ushort SamusX => unchecked((ushort)(CameraX + 128 + SamusXFromCenter));
    /// <summary>Native wrapping Y placement: camera + offset from the screen top.</summary>
    public ushort SamusY => unchecked((ushort)(CameraY + SamusYFromTop));

}
