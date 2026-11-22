using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// One title-demo scene: the room, staging and equipment of one demonstration of an <see cref="AttractDemoSet"/>.
/// </summary>
/// <param name="Room">The room the scene loads.</param>
/// <param name="Door">The door the room loads through.</param>
/// <param name="CameraPosition">The camera's initial top-left.</param>
/// <param name="Samus">How the scene stages Samus and where it places her.</param>
/// <param name="Duration">The demo frames the scene plays before the attract sequence moves on.</param>
/// <param name="RoomSetup">What the scene does to its room once it has loaded, or null when it leaves the room as loaded.</param>
/// <param name="Items">The items Samus has collected and equipped.</param>
/// <param name="Missiles">Samus's missiles and missile capacity.</param>
/// <param name="SuperMissiles">Samus's super missiles and super missile capacity.</param>
/// <param name="PowerBombs">Samus's power bombs and power bomb capacity.</param>
/// <param name="Health">Samus's energy capacity, and her energy unless her setup names less.</param>
/// <param name="CollectedBeams">The beams Samus has collected.</param>
/// <param name="EquippedBeams">The collected beams Samus has equipped.</param>
/// <param name="Input">The recording the scene plays in the player's place, and the one it continues into.</param>
public sealed partial record AttractDemoScene(
    RoomDefinition Room, DoorDefinition Door, Position16<RoomPixels> CameraPosition, AttractDemoSamusStaging Samus,
    int Duration, AttractDemoRoomSetup? RoomSetup, SamusEquipmentFlags Items, int Missiles, int SuperMissiles, int PowerBombs,
    int Health, SamusBeamFlags CollectedBeams, SamusBeamFlags EquippedBeams, AttractDemoInputDefinition Input);
