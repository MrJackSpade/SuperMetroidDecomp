namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The setup a placement's family runs as the room's population places it (#2288, #2383), with the typed argument the
/// placement gives it, decoded at import. Each family's setup is its own type, so a placement cannot carry an argument its
/// family does not read, nor lack one its family requires; the population loader hands it the room's family setups.
/// </summary>
internal abstract record RoomObjectPlacementSetup
{
    /// <summary>Only the family setups below derive from this.</summary>
    private protected RoomObjectPlacementSetup()
    {
    }

    /// <summary>Runs this setup of a placement of <paramref name="kind"/> through <paramref name="setups"/>.</summary>
    internal abstract void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind);
}

/// <summary>A coloured door's setup: its cap, locked by its placement's door, or by none.</summary>
/// <param name="LockedBy">The door the cap locks, or null for a cap that locks none.</param>
internal sealed record ColoredDoorSetup(LockedDoorArgument? LockedBy) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpColoredDoor(kind, LockedBy);
}

/// <summary>A grey door's setup, the Bomb Torizo's among them, opened by its placement's condition.</summary>
/// <param name="Opening">The condition that opens the door and the door it locks, or null for none.</param>
internal sealed record GreyDoorSetup(GreyDoorArgument? Opening) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpGreyDoor(kind, Opening);
}

/// <summary>A scroll trigger's setup: the scroll change it runs.</summary>
/// <param name="Change">The scroll change the trigger runs.</param>
internal sealed record ScrollTriggerSetup(RoomScrollChange Change) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpScrollTrigger(Change);
}

/// <summary>A scroll extension's setup, which extends the trigger beside it in its kind's direction.</summary>
internal sealed record ScrollExtensionSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpScrollExtension(kind);
}

/// <summary>A permanent item's setup: its kind shown as it is placed, unless the item it places is already collected.</summary>
/// <param name="Collected">The item location the placement places, or null for none.</param>
internal sealed record CollectibleSetup(CollectedItemArgument? Collected) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpCollectible(kind, Collected);
}

/// <summary>An eye door's part's setup, locked by its placement's door, or by none.</summary>
/// <param name="LockedBy">The door the part locks, or null for none.</param>
internal sealed record EyeDoorSetup(LockedDoorArgument? LockedBy) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpEyeDoor(kind, LockedBy);
}

/// <summary>A Draygon wall cannon's setup, controlling the wall cannon its placement names.</summary>
/// <param name="Cannon">The wall cannon the placement controls.</param>
internal sealed record DraygonWallCannonSetup(DraygonWallCannon Cannon) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpDraygonCannon(kind, Cannon);
}

/// <summary>A station's setup, a save station saving at the load station its placement names.</summary>
/// <param name="SavesAt">The load station a save station saves at, or null for a map or refill station.</param>
internal sealed record StationSetup(SaveStationArgument? SavesAt) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpStation(kind, SavesAt);
}

/// <summary>An elevator platform's setup.</summary>
internal sealed record ElevatorPlatformSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpElevatorPlatform();
}

/// <summary>Mother Brain's glass's setup.</summary>
internal sealed record MotherBrainGlassSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpMotherBrainGlass();
}

/// <summary>The Bomb Torizo statue's hand's setup.</summary>
internal sealed record BombTorizoHandSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpBombTorizoHand();
}

/// <summary>The Metroid room's clearance's setup, watching its placement's room's Metroids.</summary>
/// <param name="Watches">The event whose Metroids the clearance watches, or null for none.</param>
internal sealed record MetroidRoomClearanceSetup(MetroidRoomArgument? Watches) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpMetroidRoomClearance(Watches);
}

/// <summary>The Speed Booster's escape's setup.</summary>
internal sealed record SpeedBoosterEscapeSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpSpeedBoosterEscape();
}

/// <summary>The Wrecked Ship attic's setup.</summary>
internal sealed record WreckedShipAtticSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpWreckedShipAttic();
}

/// <summary>Maridia's glass tube's setup.</summary>
internal sealed record NoobTubeSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpNoobTube();
}

/// <summary>The setup of the gate Mother Brain's escape room shuts behind Samus.</summary>
internal sealed record MotherBrainEscapeRoomGateSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpMotherBrainEscapeRoomGate();
}

/// <summary>A downward gate's setup.</summary>
internal sealed record DownwardGateSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpDownwardGate();
}

/// <summary>A downward gate's shot block's setup, triggering its placement's gate.</summary>
/// <param name="Trigger">The gate's colour and side the shot block triggers, or null for none.</param>
internal sealed record DownwardGateShotBlockSetup(GateTriggerArgument? Trigger) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpDownwardGateShotBlock(Trigger);
}

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

    /// <summary>Sets up a Draygon wall cannon of <paramref name="kind"/> controlling <paramref name="cannon"/>.</summary>
    void SetUpDraygonCannon(RoomObjectKind kind, DraygonWallCannon cannon);

    /// <summary>Sets up a station of <paramref name="kind"/>.</summary>
    void SetUpStation(RoomObjectKind kind, SaveStationArgument? savesAt);

    /// <summary>Sets up an elevator platform.</summary>
    void SetUpElevatorPlatform();

    /// <summary>Sets up Mother Brain's glass.</summary>
    void SetUpMotherBrainGlass();

    /// <summary>Sets up the Bomb Torizo statue's hand.</summary>
    void SetUpBombTorizoHand();

    /// <summary>Sets up the Metroid room's clearance.</summary>
    void SetUpMetroidRoomClearance(MetroidRoomArgument? watches);

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
