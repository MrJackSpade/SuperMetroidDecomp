using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One title-demo scene joined across the native room, equipment, and Samus setup tables.
/// The room loader increments the global scene index after placement; these records all
/// refer to the same pre-increment index, not the following scene's equipment.
/// </summary>
/// <param name="RoomPointer">Bank-$8F pointer identifying the room loaded for this demo scene.</param>
/// <param name="DoorPointer">Door header pointer used to enter the selected room.</param>
/// <param name="CameraX">Horizontal camera origin used to place Samus and begin the scene.</param>
/// <param name="CameraY">Vertical camera origin used to place Samus and begin the scene.</param>
/// <param name="SamusYFromTop">Samus's vertical offset from the camera's top edge.</param>
/// <param name="SamusXFromCenter">Signed horizontal offset from the screen center used for Samus placement.</param>
/// <param name="Duration">Number of frames the title demo runs before advancing to its next scene.</param>
/// <param name="RoomSetupPointer">Pointer to the room setup record associated with this scene index.</param>
/// <param name="SamusSetupPointer">Pointer to Samus's setup record associated with this scene index.</param>
/// <param name="Items">Collected item bitfield applied when the scene initializes Samus.</param>
/// <param name="Missiles">Missile count initialized for the demo scene.</param>
/// <param name="SuperMissiles">Super Missile count initialized for the demo scene.</param>
/// <param name="PowerBombs">Power Bomb count initialized for the demo scene.</param>
/// <param name="Health">Health value initialized for Samus in this scene.</param>
/// <param name="CollectedBeams">Beam collection bitfield supplied by the scene equipment table.</param>
/// <param name="EquippedBeams">Beam equipment bitfield enabled for the scene.</param>
/// <param name="InputObject">Pointer to the recorded controller-input sequence played during the demo.</param>
public sealed record AttractDemoScene(
    ushort RoomPointer, ushort DoorPointer,     ushort CameraX, ushort CameraY, ushort SamusYFromTop, short SamusXFromCenter,
    ushort Duration, ushort RoomSetupPointer, ushort SamusSetupPointer,
    ushort Items, ushort Missiles, ushort SuperMissiles, ushort PowerBombs,
    ushort Health, ushort CollectedBeams, ushort EquippedBeams, ushort InputObject)
{
    /// <summary>Native wrapping X placement: camera + half-screen + signed offset.</summary>
    public ushort SamusX => unchecked((ushort)(CameraX + 128 + SamusXFromCenter));
    /// <summary>Native wrapping Y placement: camera + offset from the screen top.</summary>
    public ushort SamusY => unchecked((ushort)(CameraY + SamusYFromTop));

}
