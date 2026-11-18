namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room's room-object families' setups, which a placement's setup runs as its population places it (#2383).</summary>
internal interface IRoomObjectFamilySetups
{
    /// <summary>Sets up a coloured door of <paramref name="kind"/>.</summary>
    void SetUpColoredDoor(RoomObjectKind kind, LockedDoorArgument? lockedBy);

    /// <summary>Sets up a grey door of <paramref name="kind"/>.</summary>
    void SetUpGreyDoor(RoomObjectKind kind, GreyDoorArgument? opening);

    /// <summary>Sets up a scroll trigger running <paramref name="change"/>.</summary>
    void SetUpScrollTrigger(RoomScrollChange change);

    /// <summary>Sets up a scroll extension of <paramref name="kind"/>.</summary>
    void SetUpScrollExtension(RoomObjectKind kind);

    /// <summary>Sets up a permanent item of <paramref name="kind"/>.</summary>
    void SetUpCollectible(RoomObjectKind kind, CollectedItemArgument? collected);

    /// <summary>Sets up an eye door's part of <paramref name="kind"/>.</summary>
    void SetUpEyeDoor(RoomObjectKind kind, LockedDoorArgument? lockedBy);

    /// <summary>Sets up a Draygon wall cannon as its placement's <paramref name="setup"/> carries it.</summary>
    void SetUpDraygonCannon(DraygonWallCannonSetup setup);

    /// <summary>Sets up a station of <paramref name="kind"/>.</summary>
    void SetUpStation(RoomObjectKind kind, SaveStationArgument? savesAt);

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

    /// <summary>Sets up the Wrecked Ship attic.</summary>
    void SetUpWreckedShipAttic();

    /// <summary>Sets up Maridia's glass tube.</summary>
    void SetUpNoobTube();

    /// <summary>Sets up the gate Mother Brain's escape room shuts.</summary>
    void SetUpMotherBrainEscapeRoomGate();

    /// <summary>Sets up a downward gate.</summary>
    void SetUpDownwardGate();

    /// <summary>Sets up a downward gate's shot block.</summary>
    void SetUpDownwardGateShotBlock(GateTriggerArgument? trigger);
}
