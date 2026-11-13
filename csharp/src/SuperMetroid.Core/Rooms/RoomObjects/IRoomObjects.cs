using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's room-object owners that the room module and the domains below the room composition reach (#1395): the room-level
/// families, the shared sound queue and drawing, and the hooks the scripted room objects use. The room composition implements
/// it over every family; a domain above the room module reaches its own families through its own view.
/// </summary>
internal interface IRoomObjects
{
    /// <summary>Creates collision-triggered block room objects from Samus and projectile reactions.</summary>
    BlockReactionObjects BlockReactions { get; }

    /// <summary>The game's event flags, which scripted room objects set.</summary>
    IGlobalEventFlags? Events { get; }

    /// <summary>Queues sound requests emitted by room object cues.</summary>
    RoomObjectSoundQueue Sounds { get; }

    /// <summary>Turns room objects' block draws into level changes and background picture updates.</summary>
    IRoomObjectDrawing Drawing { get; }

    /// <summary>Handles collectible item reveals, pickup, and persistence.</summary>
    CollectibleObjects CollectibleObjects { get; }

    /// <summary>Runs save, map, and refill station room interactions.</summary>
    StationObjects Stations { get; }

    /// <summary>Door-object owner bound to this room's allocation and draw state.</summary>
    DoorObjects DoorObjects { get; }

    /// <summary>Runs eye door opening and projectile interaction cues.</summary>
    EyeDoorObjects EyeDoors { get; }

    /// <summary>Handles grey door conditions and opening animation.</summary>
    GreyDoorObjects GreyDoors { get; }

    /// <summary>Opens and closes colored gates after their matching triggers.</summary>
    GateObjects Gates { get; }

    /// <summary>Runs Samus Eater plant damage, release, and room object state.</summary>
    SamusEaterObjects SamusEater { get; }

    /// <summary>Applies room-object-driven room scroll grid changes.</summary>
    ScrollObjects Scrolls { get; }

    /// <summary>Releases the slot.</summary>
    void DeleteObject(RoomObjectSlot slot);
}
