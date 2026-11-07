using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's PLM owners that the room module and the domains below the room composition reach (#1395): the room-level
/// families, the shared sound queue and drawing, and the hooks a scripted program uses. The room composition implements
/// it over every family; a domain above the room module reaches its own families through its own view.
/// </summary>
internal interface IRoomObjects
{
    /// <summary>Creates collision-triggered block PLMs from Samus and projectile reactions.</summary>
    BlockReactionObjects BlockReactions { get; }

    /// <summary>The game's event flags, which scripted room objects set.</summary>
    IGlobalEventFlags? Events { get; }

    /// <summary>Queues sound requests emitted by room object instructions.</summary>
    RoomObjectSoundQueue Sounds { get; }

    /// <summary>Turns PLM draw-list commands into level changes and background picture updates.</summary>
    IRoomObjectDrawing Drawing { get; }

    /// <summary>Handles collectible item reveals, pickup, and persistence.</summary>
    CollectibleObjects CollectibleObjects { get; }

    /// <summary>Runs save, map, and refill station room interactions.</summary>
    StationObjects Stations { get; }

    /// <summary>Door-object instruction owner bound to this room's allocation and draw state.</summary>
    DoorObjects DoorObjects { get; }

    /// <summary>Runs eye door opening and projectile interaction instructions.</summary>
    EyeDoorObjects EyeDoors { get; }

    /// <summary>Handles grey door conditions and opening animation.</summary>
    GreyDoorObjects GreyDoors { get; }

    /// <summary>Opens and closes colored gates after their matching triggers.</summary>
    GateObjects Gates { get; }

    /// <summary>Runs the glass tube's scripted break and sound sequence.</summary>
    NoobTubeObjects NoobTube { get; }

    /// <summary>Runs Samus Eater plant damage, release, and room object state.</summary>
    SamusEaterObjects SamusEater { get; }

    /// <summary>Applies PLM-driven room scroll grid changes.</summary>
    ScrollObjects Scrolls { get; }

    /// <summary>Handles speed-booster escape tunnel block reactions.</summary>
    SpeedBoosterEscapeObjects SpeedBoosterEscape { get; }

    /// <summary>What scripted room programs call on the families that own them.</summary>
    IScriptedRoomObjectHooks ScriptedHooks { get; }

    /// <summary>Releases the slot.</summary>
    void DeleteObject(RoomObjectSlot slot);
}
