using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's room-object families' setups for one placement, once its object has taken its place (#2288, #2383): the
/// placement's own setup names its family and hands it the typed argument it reads, and the coloured, grey and eye doors,
/// the items and the stations find what their kind installs in their families' own tables.
/// </summary>
/// <param name="context">The room, the place the placement took, the area's boss flags and the families that keep state.</param>
internal sealed class RoomObjectFamilySetups(RoomObjectSetupContext context) : IRoomObjectFamilySetups
{
    /// <inheritdoc/>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> installs no coloured door.</exception>
    public void SetUpColoredDoor(RoomObjectKind kind, LockedDoorArgument? lockedBy)
    {
        ColoredDoorVariant door = DoorObjectKindDefinitions.ColoredDoors.TryGetValue(kind, out ColoredDoorVariant found) ? found
            : throw new InvalidDataException($"Room-object kind {kind} installs no coloured door.");
        DoorObjects.SetupColoredDoor(context.Room.Level, context.Place, door.Color, door.Orientation, lockedBy);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> installs no grey door.</exception>
    public void SetUpGreyDoor(RoomObjectKind kind, GreyDoorArgument? opening)
    {
        GreyDoorVariant door = DoorObjectKindDefinitions.GreyDoors.TryGetValue(kind, out GreyDoorVariant found) ? found
            : throw new InvalidDataException($"Room-object kind {kind} installs no grey door.");
        context.Families.GreyDoors.SetupGreyDoor(context.Room.Level, context.Place, door, opening);
    }

    /// <inheritdoc/>
    public void SetUpScrollTrigger(RoomScrollChange change) =>
        ScrollObjects.SetupScroll(context.Room.Level, context.Place, RoomObjectKind.ScrollTrigger, change);

    /// <inheritdoc/>
    public void SetUpScrollExtension(RoomObjectKind kind) =>
        ScrollObjects.SetupScroll(context.Room.Level, context.Place, kind, change: null);

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> places no permanent item.</exception>
    public void SetUpCollectible(RoomObjectKind kind, CollectedItemArgument? collected)
    {
        PermanentCollectibleObject item = PermanentCollectibleObjectDefinitions.Items.TryGetValue(kind, out PermanentCollectibleObject found) ? found
            : throw new InvalidDataException($"Room-object kind {kind} places no permanent item.");
        context.Families.CollectibleObjects.SetupCollectible(context.Room, context.Place, item.Kind, item.Presentation, collected);
    }

    /// <inheritdoc/>
    public void SetUpEyeDoor(RoomObjectKind kind, LockedDoorArgument? lockedBy) =>
        context.Families.EyeDoors.SetupEyeDoor(context.Room.Level, context.Place, kind, lockedBy);

    /// <inheritdoc/>
    public void SetUpDraygonCannon(DraygonWallCannonSetup setup) =>
        context.Families.DraygonCannons.SetupDraygonCannon(context.Room.Level, context.Place, setup);

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> installs no station.</exception>
    public void SetUpStation(RoomObjectKind kind, SaveStationArgument? savesAt)
    {
        StationKind station = StationKindDefinitions.Kinds.TryGetValue(kind, out StationKind found) ? found
            : throw new InvalidDataException($"Room-object kind {kind} installs no station.");
        StationObjects.SetupStation(context.Room.Level, context.Room.System, context.Room.Area, context.Place, station, savesAt?.Station);
    }

    /// <inheritdoc/>
    public void SetUpElevatorPlatform() => StationObjects.SetupElevatorPlatform(context.Room.Level, context.Place);

    /// <inheritdoc/>
    public void SetUpMotherBrainGlass() => context.Families.MotherBrainGlass.SetupMotherBrainGlass(context.Room.Level, context.Place);

    /// <inheritdoc/>
    public void SetUpBombTorizoHand() => context.Families.BombTorizoHand.SetupBombTorizoHand(context.Place, context.Bosses);

    /// <inheritdoc/>
    public void SetUpMetroidRoomClearance(MetroidRoomArgument watches) => TourianObjects.SetupMetroidsCleared(context.Place, watches);

    /// <inheritdoc/>
    /// <remarks>The held place's object changes nothing: it only holds its place.</remarks>
    public void SetUpEscapeRoomHeldPlace() => context.Place.Become(new EscapeRoomHeldPlaceObject());

    /// <inheritdoc/>
    public void SetUpSpeedBoosterEscape() => context.Families.SpeedBoosterEscape.SetupSpeedBoosterEscape(context.Place);

    /// <inheritdoc/>
    /// <remarks>The attic's held place's object changes nothing: it only holds its place.</remarks>
    public void SetUpWreckedShipAtticHeldPlace() => context.Place.Become(new WreckedShipAtticHeldPlaceObject());

    /// <inheritdoc/>
    public void SetUpNoobTube() => context.Families.NoobTube.SetupNoobTube(context.Room.Level, context.Place);

    /// <inheritdoc/>
    /// <remarks>The gate is a passable block, shut.</remarks>
    public void SetUpMotherBrainEscapeRoomGate()
    {
        DoorObjects.SetupPassableBlock(context.Room.Level, context.Place);
        context.Place.Become(new MotherBrainEscapeGateObject(MotherBrainEscapeGateDefinitions.Shut));
    }

    /// <inheritdoc/>
    public void SetUpDownwardGate() => context.Families.Gates.SetupDownwardGate(context.Room.Level, context.Place);

    /// <inheritdoc/>
    public void SetUpDownwardGateShotBlock(GateTriggerArgument? trigger) =>
        GateObjects.SetupDownwardGateShotBlock(context.Room.Level, context.Place, trigger);
}
