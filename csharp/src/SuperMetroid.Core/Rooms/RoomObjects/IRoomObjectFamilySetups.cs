using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's room-object families' setups, which a placement's setup runs as its population places it (#2383), each
/// handed what its placement decoded (#2481).
/// </summary>
internal interface IRoomObjectFamilySetups
{
    /// <summary>Sets up a coloured door's cap, <paramref name="door"/>.</summary>
    void SetUpColoredDoor(ColoredDoorVariant door, LockedDoorArgument? lockedBy);

    /// <summary>Sets up a grey door's cap, <paramref name="door"/>.</summary>
    void SetUpGreyDoor(GreyDoorVariant door, GreyDoorArgument? opening);

    /// <summary>Sets up a scroll trigger running <paramref name="change"/>.</summary>
    void SetUpScrollTrigger(RoomScrollChange change);

    /// <summary>Sets up a scroll extension writing <paramref name="extension"/>.</summary>
    void SetUpScrollExtension(ScrollExtension extension);

    /// <summary>Sets up a permanent item, <paramref name="item"/>.</summary>
    void SetUpCollectible(PermanentCollectibleObject item, CollectedItemArgument? collected);

    /// <summary>Sets up an eye door's eye facing <paramref name="orientation"/>.</summary>
    void SetUpEyeDoorEye(EyeDoorOrientation orientation, LockedDoorArgument lockedBy);

    /// <summary>Sets up an eye door's body or bottom, <paramref name="part"/>.</summary>
    void SetUpEyeDoorPart(EyeDoorPart part, LockedDoorArgument? lockedBy);

    /// <summary>Sets up a Draygon wall cannon as its placement's <paramref name="setup"/> carries it.</summary>
    void SetUpDraygonCannon(DraygonWallCannonSetup setup);

    /// <summary>Sets up a station, <paramref name="station"/>.</summary>
    void SetUpStation(StationKind station, SaveStationArgument? savesAt);

    /// <summary>Sets up an elevator platform.</summary>
    void SetUpElevatorPlatform();

    /// <summary>Sets up Mother Brain's glass.</summary>
    void SetUpMotherBrainGlass();

    /// <summary>Sets up the Bomb Torizo statue's hand.</summary>
    void SetUpBombTorizoHand();

    /// <summary>Sets up the Metroid room's clearance.</summary>
    void SetUpMetroidRoomClearance(MetroidRoomArgument watches);

    /// <summary>Sets up an escape room's held place.</summary>
    void SetUpEscapeRoomHeldPlace();

    /// <summary>Sets up the Speed Booster's escape.</summary>
    void SetUpSpeedBoosterEscape();

    /// <summary>Sets up the Wrecked Ship attic's held place.</summary>
    void SetUpWreckedShipAtticHeldPlace();

    /// <summary>Sets up Maridia's glass tube.</summary>
    void SetUpNoobTube();

    /// <summary>Sets up the gate Mother Brain's escape room shuts.</summary>
    void SetUpMotherBrainEscapeRoomGate();

    /// <summary>Sets up a downward gate.</summary>
    void SetUpDownwardGate();

    /// <summary>Sets up a downward gate's shot block.</summary>
    void SetUpDownwardGateShotBlock(GateTriggerArgument? trigger);
}
