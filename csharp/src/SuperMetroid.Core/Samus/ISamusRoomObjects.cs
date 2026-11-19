using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The room objects Samus's movement, collision, grapple and projectiles reach when a block they touch reacts (#1395, #2490):
/// the families those reactions spawn or notify and whether the room's object places are all held, nothing else of the
/// room's room objects. The room composition implements it; the intro's collision rooms, which hold no reacting block,
/// implement it as rooms no reaction reaches.
/// </summary>
internal interface ISamusRoomObjects
{
    /// <summary>Creates the block room objects Samus's and her shots' contacts with reacting blocks spawn.</summary>
    BlockReactionObjects BlockReactions { get; }

    /// <summary>Collectible item reveals and pickups that Samus touches and her shots hit.</summary>
    CollectibleObjects CollectibleObjects { get; }

    /// <summary>The save, map and refill stations Samus's collision touches.</summary>
    StationObjects Stations { get; }

    /// <summary>The blue doors her shots and grapple open.</summary>
    DoorObjects DoorObjects { get; }

    /// <summary>The coloured gates her shots and grapple trigger.</summary>
    GateObjects Gates { get; }

    /// <summary>The Samus Eater plants Samus's inside-block reactions spawn.</summary>
    SamusEaterObjects SamusEater { get; }

    /// <summary>The scroll triggers Samus touches.</summary>
    ScrollObjects Scrolls { get; }

    /// <summary>Spawns room objects for breakable blocks struck by projectile impacts.</summary>
    ProjectileTriggerObjects ProjectileTriggers { get; }

    /// <summary>Runs quicksand room objects, which take the room's room-object places.</summary>
    QuicksandObjects Quicksand { get; }

    /// <summary>The Chozo statue hand Samus's collision notifies.</summary>
    IChozoStatueHand ChozoStatueHand { get; }

    /// <summary>
    /// The room's door contacts, which Samus's collision with a door block meets (#2545): the door she enters and her
    /// stand at the elevator's door.
    /// </summary>
    RoomDoorContacts DoorContacts { get; }

    /// <summary>
    /// True when every one of the room's room-object places is held. A block contact whose reaction would take a place then
    /// runs none of its setup, so even a block whose setup would stop Samus lets her pass.
    /// </summary>
    bool EveryPlaceHeld { get; }
}
