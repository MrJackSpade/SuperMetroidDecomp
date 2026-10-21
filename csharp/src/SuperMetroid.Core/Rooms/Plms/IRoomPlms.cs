using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The room's PLM owners that the room module and the domains below the room composition reach (#1395): the room-level
/// families, the shared sound queue and drawing, and the hooks a scripted program uses. The room composition implements
/// it over every family; a domain above the room module reaches its own families through its own view.
/// </summary>
internal interface IRoomPlms
{
    /// <summary>Creates collision-triggered block PLMs from Samus and projectile reactions.</summary>
    BlockReactionPlms BlockReactions { get; }

    /// <summary>The game's event flags, which scripted room objects set.</summary>
    IGlobalEventFlags? Events { get; }

    /// <summary>Queues sound requests emitted by room object instructions.</summary>
    PlmSoundQueue Sounds { get; }

    /// <summary>Turns PLM draw-list commands into level changes and BG tilemap updates.</summary>
    IPlmDrawing Drawing { get; }

    /// <summary>Handles collectible item reveals, pickup, and persistence.</summary>
    CollectiblePlms CollectiblePlms { get; }

    /// <summary>Runs save, map, and refill station room interactions.</summary>
    StationPlms Stations { get; }

    /// <summary>Door-object instruction owner bound to this room's allocation and draw state.</summary>
    DoorPlms DoorPlms { get; }

    /// <summary>Runs eye door opening and projectile interaction instructions.</summary>
    EyeDoorPlms EyeDoors { get; }

    /// <summary>Handles grey door conditions and opening animation.</summary>
    GreyDoorPlms GreyDoors { get; }

    /// <summary>Opens and closes colored gates after their matching triggers.</summary>
    GatePlms Gates { get; }

    /// <summary>Runs the glass tube's scripted break and sound sequence.</summary>
    NoobTubePlms NoobTube { get; }

    /// <summary>Runs Samus Eater plant damage, release, and room object state.</summary>
    SamusEaterPlms SamusEater { get; }

    /// <summary>Applies PLM-driven room scroll grid changes.</summary>
    ScrollPlms Scrolls { get; }

    /// <summary>Handles speed-booster escape tunnel block reactions.</summary>
    SpeedBoosterEscapePlms SpeedBoosterEscape { get; }

    /// <summary>What scripted room programs call on the families that own them.</summary>
    IScriptedRoomPlmHooks ScriptedHooks { get; }

    /// <summary><c>$84:86BC Instruction_PLM_Delete</c>: releases the slot.</summary>
    void DeletePlm(PlmSlot slot);
}
