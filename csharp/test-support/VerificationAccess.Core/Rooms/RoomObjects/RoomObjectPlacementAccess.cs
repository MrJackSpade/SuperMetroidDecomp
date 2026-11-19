using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Verification's view of a placement's typed setup (#2383): the room argument, the Draygon wall cannon and the scroll
/// change it carries, and the placement a decoded native record makes, its kind's family setup holding what the record
/// gives it, as the population catalog's generator writes it.
/// </summary>
internal static class RoomObjectPlacementAccess
{
    /// <summary>What a placement's setup carries.</summary>
    extension(RoomObjectPlacement placement)
    {
        /// <summary>The room argument the placement's setup reads, or null for one that reads none.</summary>
        internal RoomObjectRoomArgument? Argument => placement.Setup switch
        {
            ColoredDoorSetup door => door.LockedBy,
            GreyDoorSetup door => door.Opening,
            CollectibleSetup item => item.Collected,
            EyeDoorEyeSetup eye => eye.LockedBy,
            EyeDoorPartSetup part => part.LockedBy,
            SaveStationSetup station => station.SavesAt,
            MetroidRoomClearanceSetup clearance => clearance.Watches,
            DownwardGateShotBlockSetup shotBlock => shotBlock.Trigger,
            _ => null,
        };

        /// <summary>The Draygon wall cannon a cannon placement controls, or null for every other placement.</summary>
        internal DraygonWallCannon? Cannon => placement.Setup is DraygonWallCannonSetup cannon ? cannon.Cannon : null;

        /// <summary>The scroll change a scroll trigger runs, or null for every other placement.</summary>
        internal RoomScrollChange? ScrollChange => placement.Setup is ScrollTriggerSetup trigger ? trigger.Change : null;
    }

    /// <summary>
    /// The placement of <paramref name="kind"/> at <paramref name="block"/> a decoded record makes: its family's setup
    /// holding <paramref name="argument"/>, <paramref name="cannon"/> or <paramref name="scrollChange"/>, as its family reads.
    /// </summary>
    /// <exception cref="InvalidDataException">The record gives the kind's family what it does not read, or lacks what it requires.</exception>
    internal static RoomObjectPlacement Placed(RoomObjectKind kind, Position16<RoomBlocks> block, RoomObjectRoomArgument? argument = null,
        DraygonWallCannon? cannon = null, RoomScrollChange? scrollChange = null) =>
        new(kind, block, SetupOf(kind, argument, cannon, scrollChange));

    /// <summary>The family setup of <paramref name="kind"/> holding what its record gives it.</summary>
    /// <exception cref="InvalidDataException">The record gives the kind's family what it does not read, or lacks what it requires.</exception>
    internal static RoomObjectPlacementSetup SetupOf(RoomObjectKind kind, RoomObjectRoomArgument? argument, DraygonWallCannon? cannon,
        RoomScrollChange? scrollChange)
    {
        if (kind == RoomObjectKind.ScrollTrigger)
            return new ScrollTriggerSetup(scrollChange ?? throw new InvalidDataException("A scroll trigger needs the scroll change it applies."));
        if (scrollChange is not null)
            throw new InvalidDataException("Only a scroll trigger may apply a scroll change.");
        if (DraygonCannonObjectsConstants.CannonKinds.TryGetValue(kind, out (DraygonCannonOrientation Orientation, DraygonCannonStart Start) wall))
        {
            return new DraygonWallCannonSetup(cannon ?? throw new InvalidDataException($"Draygon cannon placement {kind} names no Draygon wall cannon."),
                wall.Orientation, wall.Start);
        }
        if (cannon is not null)
            throw new InvalidDataException($"Placement {kind} is no Draygon cannon to control a wall cannon.");
        if (RoomObjectKindVariants.ColoredDoors.TryGetValue(kind, out ColoredDoorVariant colored))
            return new ColoredDoorSetup(colored, Require<DoorLock>(kind, argument));
        if (RoomObjectKindVariants.GreyDoors.TryGetValue(kind, out GreyDoorVariant grey))
            return new GreyDoorSetup(grey, Require<GreyDoorOpening>(kind, argument));
        if (RoomObjectKindVariants.Items.TryGetValue(kind, out PermanentCollectibleObject item))
            return new CollectibleSetup(item, Require<CollectibleIdentity>(kind, argument));
        if (RoomObjectKindVariants.EyeDoorEyes.TryGetValue(kind, out EyeDoorOrientation eye))
            return new EyeDoorEyeSetup(eye, Require<DoorLock>(kind, argument));
        if (RoomObjectKindVariants.EyeDoorParts.TryGetValue(kind, out EyeDoorPart part))
            return new EyeDoorPartSetup(part, Require<DoorLock>(kind, argument));
        if (kind == RoomObjectKind.SaveStation)
            return new SaveStationSetup(Require<SaveStationPlace>(kind, argument));
        if (RoomObjectKindVariants.AccessStations.TryGetValue(kind, out StationAccessRule? station))
        {
            return argument is null ? new AccessStationSetup(station)
                : throw new InvalidDataException($"Placement {kind} reads no argument, but its record gives {argument}.");
        }
        if (RoomObjectKindVariants.ScrollExtensions.TryGetValue(kind, out SuperMetroid.Core.Rooms.Scrolling.ScrollExtension extension))
        {
            return argument is null ? new ScrollExtensionSetup(extension)
                : throw new InvalidDataException($"Placement {kind} reads no argument, but its record gives {argument}.");
        }
        if (kind == RoomObjectKind.MetroidRoomClearance)
            return new MetroidRoomClearanceSetup(Require<WatchedMetroidRoom>(kind, argument));
        if (kind == RoomObjectKind.DownwardGateShotBlock) return new DownwardGateShotBlockSetup(Require<GateShotTrigger>(kind, argument));
        if (argument is not null)
            throw new InvalidDataException($"Placement {kind} reads no argument, but its record gives {argument}.");
        return UnarguedSetups.TryGetValue(kind, out RoomObjectPlacementSetup? setup) ? setup
            : throw new InvalidDataException($"No room-object family sets up placement {kind}.");
    }

    /// <summary>The setup of each kind whose family reads no argument.</summary>
    private static readonly Dictionary<RoomObjectKind, RoomObjectPlacementSetup> UnarguedSetups = new()
    {
        [RoomObjectKind.ElevatorPlatform] = new ElevatorPlatformSetup(),
        [RoomObjectKind.MotherBrainGlass] = new MotherBrainGlassSetup(),
        [RoomObjectKind.BombTorizoHand] = new BombTorizoHandSetup(),
        [RoomObjectKind.SpeedBoosterEscape] = new SpeedBoosterEscapeSetup(),
        [RoomObjectKind.WreckedShipAtticHeldPlace] = new WreckedShipAtticHeldPlaceSetup(),
        [RoomObjectKind.NoobTube] = new NoobTubeSetup(),
        [RoomObjectKind.MotherBrainEscapeRoomGate] = new MotherBrainEscapeRoomGateSetup(),
        [RoomObjectKind.DownwardGate] = new DownwardGateSetup(),
        [RoomObjectKind.EscapeRoomHeldPlace] = new EscapeRoomHeldPlaceSetup(),
    };

    /// <summary>The <typeparamref name="T"/> argument a family of <paramref name="kind"/> requires.</summary>
    /// <exception cref="InvalidDataException">The record gives no argument, or one of another kind.</exception>
    private static T Require<T>(RoomObjectKind kind, RoomObjectRoomArgument? argument) where T : RoomObjectRoomArgument => argument switch
    {
        null => throw new InvalidDataException($"Placement {kind} requires a {typeof(T).Name}, but its record gives none."),
        T typed => typed,
        _ => throw new InvalidDataException($"Placement {kind} reads a {typeof(T).Name}, but its record gives {argument}."),
    };
}
