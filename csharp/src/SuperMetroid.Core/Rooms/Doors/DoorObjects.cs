using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room's doors as room objects: the coloured caps' setup, the blue cap a shot opens, the record of a door opened for
/// good, and the closing behind Samus after a door transition.
/// </summary>
[RoomLifetime]
internal sealed class DoorObjects
{
    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Allocates fallback closing and blue-door actors from the room-object pool.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>Queues the enemy-library response to rejected colored-door hits.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds door room-object logic to room persistence, drawing, allocation, and sound owners.</summary>
    internal DoorObjects(SharedGameState? system, RoomObjectSlotTable slotTable, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>
    /// Closes the door Samus entered through behind her, after the destination room's objects and its door and setup
    /// code have been placed.
    /// </summary>
    /// <remarks>
    /// The first object at the door's block, in the room's update order, closes unless it is a cap whose door has been
    /// opened for good: a coloured or grey cap, an eye door's eye or Mother Brain's escape gate closes from its next update.
    /// With no object there, the door's closing behaviour may place a blue cap closing toward the travel direction or the
    /// escape gate closing, in the first free place, like any other room object.
    /// </remarks>
    /// <returns>
    /// True when an object at the door closes or a closer was placed; false for a door that does not close or a full
    /// room.
    /// </returns>
    public bool TrySpawnDoorClosingObject(
        RoomLevelData level,
        DoorDefinition door,
        SharedGameState system)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(door);
        Ensure.NotNull(system);

        Position16<RoomBlocks> block = level.GridBlock(door.RoomObjectBlock);
        foreach (RoomObjectSlot resident in _slotTable.InProcessingOrder)
        {
            if (!resident.Active || resident.Block != block)
                continue;

            // A cap whose opening never persists always closes. The escape entrance's cap never persists, so it
            // always selects its closing sequence. Ordinary coloured caps consult the doors opened for good.
            if (resident.Argument is DoorBitArgument { Persists: true } residentDoorBit &&
                system.HasOpenedDoor(residentDoorBit.Door))
            {
                break;
            }

            // The closing keeps everything else the object holds: a cap that waits for a shot still does.
            (resident.Behaviour as IClosingDoor ?? throw new InvalidDataException(
                $"The room object at the door's block {block} has no closing behind Samus.")).CloseBehindSamus();
            return true;
        }

        if (DoorClosingObjects.GetDefinition(door.Orientation) is not { } fallback)
            return false;

        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Kind = fallback.Kind;
        slot.Block = block;
        slot.Become(fallback.BlueCapFacing is { } facing
            ? new BlueDoorCapObject(facing, BlueDoorCapPhase.Closing)
            : new MotherBrainEscapeGateObject(MotherBrainEscapeGateDefinitions.Closing));
        SetupDoorTransitionDeactivatedSlot(level, slot);
        return true;
    }

    /// <summary>
    /// The deactivating setup: a solid block stays solid and any other block becomes air, keeping its appearance.
    /// Despite its historical name, the setup does not delete or deactivate the room-object slot.
    /// </summary>
    internal static void SetupDoorTransitionDeactivatedSlot(RoomLevelData level, RoomObjectSlot slot) =>
        level.SetTerrain(slot.Block, level.GetCollisionBlock(slot.Block).Terrain.Deactivated());

    /// <summary>Installs shootable-solid collision and the resident room-object projectile behaviour at a colored cap.</summary>
    private static void ApplyColoredDoorSetup(RoomLevelData level, Position16<RoomBlocks> block)
    {
        // The colored-door setup keeps the block's appearance, installs shootable-solid collision, and selects the
        // resident room-object projectile trigger so subsequent projectiles find this resident room object.
        level.SetObjectTerrain(
            block,
            level.GetObjectCollisionBlock(block).Terrain.WithCollision(RoomCollisionType.ShootableBlock));
        level.SetObjectBehavior(block, RoomBlockBehavior.Reacting(BlockReactionDefinitions.ResidentObjectTrigger));
    }

    /// <summary>
    /// Sets up a coloured cap: installs its shootable collision and places the cap, which takes its first look on its
    /// first update.
    /// </summary>
    internal static void SetupColoredDoorSlot(
        RoomLevelData level,
        RoomObjectSlot slot,
        ColoredDoorColor color,
        ColoredDoorOrientation orientation)
    {
        ApplyColoredDoorSetup(level, slot.Block);
        slot.Become(new ColoredDoorObject(new ColoredDoorVariant(color, orientation)));
    }

    /// <summary>Queues the dud sound a shot a cap does not take makes.</summary>
    internal void QueueDudSound() =>
        _sounds.Queue(EnemySound.DudShotOnDoor, Priority: DudSoundPriority);

    /// <summary>The dud sound's request priority.</summary>
    private static readonly SoundRequestPriority DudSoundPriority = SoundRequestPriority.Normal;

    /// <summary>
    /// Whether this cap's door has been opened for good before; a cap whose opening never persists never has.
    /// </summary>
    internal bool IsDoorOpened(RoomObjectSlot slot) =>
        slot.Argument is DoorBitArgument { Persists: true } cap && RequireSystem().HasOpenedDoor(cap.Door);

    /// <summary>
    /// Records the door of the cap at <paramref name="place"/> opened for good, when its opening persists, and marks the
    /// cap spent: it then keeps only its own door.
    /// </summary>
    internal void RecordDoorOpened(RoomObjectSlot place)
    {
        Ensure.NotNull(place);
        if (place.Argument is DoorBitArgument { Persists: true } cap)
        {
            RequireSystem().OpenDoor(cap.Door);
            place.Argument = SpentDoorArgument.Instance;
        }
    }

    /// <summary>The room's door-bit persistence owner.</summary>
    private SharedGameState RequireSystem() =>
        _system ?? throw new InvalidOperationException("A resident door has no persistence owner.");

    /// <summary>
    /// Names the color and facing of a colored-door room object (#1414: each of the twelve doors is matched by member, replacing
    /// the color ranges the room-object definitions implied).
    /// </summary>
    internal static bool TryIdentifyColoredDoor(
        RoomObjectKind kind,
        out ColoredDoorColor color,
        out ColoredDoorOrientation orientation)
    {
        bool found = DoorObjectKindDefinitions.ColoredDoors.TryGetValue(kind, out ColoredDoorVariant variant);
        color = variant.Color;
        orientation = variant.Orientation;
        return found;
    }

    /// <summary>
    /// Places the blue cap a shot opens, selected by a shootable block's blue-door behaviour, and applies its setup.
    /// </summary>
    /// <remarks>
    /// The cap's top or left block begins as a shootable solid. Setup changes only that block to a plain solid,
    /// preserving its appearance; the cap's opening then draws all four of its blocks and ends on the cleared opening
    /// (<see cref="BlueDoorCapObject"/>). A Power Bomb is the one projectile that does not open it. A full room loses the
    /// request, as the game's block-spawned objects do.
    /// </remarks>
    public bool TrySpawnBlueDoorOpening(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        if (!behavior.TryGetBlueDoorOrientation(out ColoredDoorOrientation orientation))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                behavior,
                "The shootable block's behaviour is not a blue-door facing.");
        }

        // A power-bomb collision deletes the just-allocated room object without touching the cap.
        if (projectileType.Family == SamusProjectileFamily.PowerBomb)
            return false;

        RoomObjectKind blueDoorKind = orientation switch
        {
            ColoredDoorOrientation.Left => RoomObjectKind.BlueDoorFacingLeft,
            ColoredDoorOrientation.Right => RoomObjectKind.BlueDoorFacingRight,
            ColoredDoorOrientation.Up => RoomObjectKind.BlueDoorFacingUp,
            ColoredDoorOrientation.Down => RoomObjectKind.BlueDoorFacingDown,
            _ => throw new InvalidOperationException(
                "A validated blue-door facing escaped its four-way kind table."),
        };

        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Kind = blueDoorKind;
        slot.Block = position;
        slot.Become(new BlueDoorCapObject(orientation, BlueDoorCapPhase.Opening));
        // The spawn leaves the argument the game's first door, the Landing Site's green door, which a later closing
        // check reads: once that door has been opened for good, the spawned blue door never closes.
        slot.Argument = new DoorBitArgument(LockedDoorDefinitions.LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombsGreenDoorFacingLeft,
            DoorOpeningPersistence.Persistent);

        // This is a direct terrain write, not a cap draw. The cap's first draw comes on its next update, while collision
        // sees the solid block at once.
        level.SetTerrain(position,
            level.GetCollisionBlock(position).Terrain.WithCollision(RoomCollisionType.SolidBlock));
        return true;
    }
}
