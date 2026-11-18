using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// One title-demo scene joined across the room, equipment, and Samus setup definitions: the room, staging and
/// equipment of one demonstration of an <see cref="AttractDemoSet"/>.
/// </summary>
/// <remarks>
/// <c>Duration</c> is the number of demo frames the scene plays before the attract sequence moves on.
/// <paramref name="RoomSetup"/> is what the scene does to its room once it has loaded, or null when it leaves the room as loaded.
/// <paramref name="CameraPosition"/> is the initial camera top-left; <paramref name="SamusPosition"/> is the room position
/// the scene places Samus at.
/// </remarks>
public sealed record AttractDemoScene(
    RoomDefinition Room, DoorDefinition Door, Position16<RoomPixels> CameraPosition, Position16<RoomPixels> SamusPosition,
    int Duration, AttractDemoRoomSetup? RoomSetup, AttractDemoSamusSetup SamusSetup,
    SuperMetroid.Core.Progression.SamusEquipmentFlags Items, int Missiles, int SuperMissiles, int PowerBombs,
    int Health, SuperMetroid.Core.Progression.SamusBeamFlags CollectedBeams, SuperMetroid.Core.Progression.SamusBeamFlags EquippedBeams, AttractDemoInputDefinition Input);
