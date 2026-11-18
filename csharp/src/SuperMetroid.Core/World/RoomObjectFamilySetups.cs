using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's room-object families' setups for one placement, once its object has taken its place (#2288, #2383): the
/// placement's own setup names its family and hands it what its placement decoded (#2481), the door, item or station it
/// places among it.
/// </summary>
/// <param name="context">The room, the place the placement took, the area's boss flags and the families that keep state.</param>
internal sealed class RoomObjectFamilySetups(RoomObjectSetupContext context) : IRoomObjectFamilySetups
{
    /// <inheritdoc/>
    public void SetUpColoredDoor(ColoredDoorVariant door, LockedDoorArgument? lockedBy) =>
        DoorObjects.SetupColoredDoor(context.Room.Level, context.Place, door, lockedBy);

    /// <inheritdoc/>
    public void SetUpGreyDoor(GreyDoorVariant door, GreyDoorArgument? opening) =>
        GreyDoorObjects.SetupGreyDoor(context.Room.Level, context.Place, door, opening);

    /// <inheritdoc/>
    public void SetUpScrollTrigger(RoomScrollChange change) =>
        ScrollObjects.SetupScrollTrigger(context.Room.Level, context.Place, change);

    /// <inheritdoc/>
    public void SetUpScrollExtension(ScrollExtension extension) =>
        ScrollObjects.SetupScrollExtension(context.Room.Level, context.Place, extension);

    /// <inheritdoc/>
    public void SetUpCollectible(PermanentCollectibleObject item, CollectedItemArgument? collected) =>
        context.Families.CollectibleObjects.SetupCollectible(context.Room, context.Place, item, collected);

    /// <inheritdoc/>
    public void SetUpEyeDoorEye(EyeDoorOrientation orientation, LockedDoorArgument lockedBy) =>
        context.Families.EyeDoors.SetupEyeDoorEye(context.Room.Level, context.Place, orientation, lockedBy);

    /// <inheritdoc/>
    public void SetUpEyeDoorPart(EyeDoorPart part, LockedDoorArgument? lockedBy) =>
        context.Families.EyeDoors.SetupEyeDoorPart(context.Room.Level, context.Place, part, lockedBy);

    /// <inheritdoc/>
    public void SetUpDraygonCannon(DraygonWallCannonSetup setup) =>
        context.Families.DraygonCannons.SetupDraygonCannon(context.Room.Level, context.Place, setup);

    /// <inheritdoc/>
    public void SetUpAccessStation(StationKind station) =>
        context.Families.Stations.SetupAccessStation(context.Room.Level, context.Room.Area, context.Place, station);

    /// <inheritdoc/>
    public void SetUpSaveStation(SaveStationArgument? savesAt) =>
        StationObjects.SetupSaveStation(context.Room.Level, context.Place, savesAt?.Station);

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
