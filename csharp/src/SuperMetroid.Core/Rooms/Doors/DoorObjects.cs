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
    private readonly SharedGameState _system;
    /// <summary>The room's object places, where a door's closing cap and a blue door's cap each take one.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>Queues the enemy-library response to rejected colored-door hits.</summary>
    private readonly RoomObjectSoundQueue _sounds;
    /// <summary>Whether a blue cap a shot opened closes behind Samus.</summary>
    private readonly ShotOpenedCapClosing<LockedDoor> _shotCapClosing;

    /// <summary>Binds door room-object logic to room persistence, the room's places, sound and the shot cap's closing rule.</summary>
    internal DoorObjects(SharedGameState system, RoomObjectPlaceList places, RoomObjectSoundQueue sounds,
        ShotOpenedCapClosing<LockedDoor> shotCapClosing)
    {
        _system = system;
        _places = places;
        _sounds = sounds;
        _shotCapClosing = Ensure.NotNull(shotCapClosing);
    }
    /// <summary>
    /// Closes the door Samus entered through behind her, once the destination room's objects are placed and what its door
    /// and the room do on entry has run.
    /// </summary>
    /// <remarks>
    /// The first door at the door's block, in the room's update order (<see cref="IDoorBehindSamus"/>, #2493), closes
    /// unless it stays open (#2091): a cap or eye door's body whose door has been opened for good stays open, and so does a
    /// blue cap a shot opened while the room's <see cref="ShotOpenedCapClosing{TDoor}"/> rule keeps it open (#1906).
    /// Otherwise a coloured or grey cap, a blue cap a shot opened, an eye door's body or Mother Brain's escape gate closes
    /// from its next update. With no door there, or one that stays open, the door's closing behaviour may place a blue cap
    /// closing toward the travel direction or the escape gate closing, in the first free place, like any other room object.
    /// </remarks>
    /// <returns>
    /// True when an object at the door closes or a closer was placed; false for a door that does not close or a full
    /// room.
    /// </returns>
    public bool TrySpawnDoorClosingObject(RoomLevelData level, DoorDefinition door)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(door);

        Position16<RoomBlocks> block = level.GridBlock(door.RoomObjectBlock);
        foreach (RoomObjectPlace resident in _places.InUpdateOrder)
        {
            if (resident.Door is not { } standing || resident.Block != block)
                continue;

            // A cap whose opening never persists always closes. The escape entrance's cap never persists, so it
            // always selects its closing sequence. Ordinary coloured caps consult the doors opened for good.
            if (standing.StaysOpenBehindSamus(_system.HasOpenedDoor))
                break;

            // The closing keeps everything else the object holds: a cap that waits for a shot still does.
            standing.CloseBehindSamus();
            return true;
        }

        if (DoorClosingObjects.GetDefinition(door.Orientation) is not { } fallback)
            return false;

        if (_places.Spawn() is not { } place)
            return false;

        place.Block = block;
        place.Become(fallback.BlueCapFacing is { } facing
            ? BlueDoorCapObject.Closing(facing)
            : new MotherBrainEscapeGateObject(MotherBrainEscapeGateDefinitions.Closing));
        SetupPassableBlock(level, place);
        return true;
    }

    /// <summary>
    /// The setup that makes a room object's block passable: a solid block stays solid and any other block becomes air. Only
    /// its collision kind changes, so it keeps its picture and what it shows (#2502). The room object keeps its place.
    /// </summary>
    internal static void SetupPassableBlock(RoomLevelData level, RoomObjectPlace place) => level.Deactivate(place.Block);

    /// <summary>Installs shootable-solid collision and the resident room-object projectile behaviour at a colored cap.</summary>
    private static void ApplyColoredDoorSetup(RoomLevelData level, Position16<RoomBlocks> block)
    {
        // The colored-door setup installs shootable-solid collision and selects the resident room-object projectile trigger
        // so subsequent projectiles find this resident room object. It rewrites only the block's type and parameter, so the
        // block keeps its picture and what it shows (#2502).
        level.SetObjectCollisionAndBehavior(block, RoomCollisionType.ShootableBlock,
            RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger));
    }

    /// <summary>
    /// Sets up a coloured cap of <paramref name="variant"/>: installs its shootable collision and places the cap, which
    /// takes its first look on its first update.
    /// </summary>
    internal static void SetupColoredDoor(
        RoomLevelData level,
        RoomObjectPlace place,
        ColoredDoorVariant variant,
        LockedDoorArgument door)
    {
        ApplyColoredDoorSetup(level, place.Block);
        place.BecomeDoor(new ColoredDoorObject(variant, new PlacedDoor(door)));
    }

    /// <summary>Queues the dud sound a shot a cap does not take makes.</summary>
    internal void QueueDudSound() =>
        _sounds.Queue(EnemySound.DudShot, Priority: DudSoundPriority);

    /// <summary>The dud sound's request priority.</summary>
    private static readonly SoundRequestPriority DudSoundPriority = SoundRequestPriority.Normal;

    /// <summary>
    /// Whether <paramref name="door"/> has been opened for good before; a door whose opening never persists never has, nor
    /// has a spent object's.
    /// </summary>
    internal bool IsDoorOpened(PlacedDoor door) => Ensure.NotNull(door).IsOpenedForGood(_system.HasOpenedDoor);

    /// <summary>
    /// Records <paramref name="door"/> opened for good, when its opening persists, which spends its object.
    /// </summary>
    internal void RecordDoorOpened(PlacedDoor door) => Ensure.NotNull(door).RecordOpened(_system);

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

        // A Power Bomb opens no blue cap: the cap stays shut and no room object takes a place.
        if (projectileType.Family == SamusProjectileFamily.PowerBomb)
            return false;

        if (_places.Spawn() is not { } place)
            return false;

        place.Block = position;
        // A later closing behind Samus at this block asks the room's shot-cap rule whether the cap stays open (#1906), and
        // closes it otherwise (#2493).
        place.BecomeDoor(BlueDoorCapObject.Opening(orientation, _shotCapClosing));

        // The block turns solid at once, before the cap is drawn. The cap's first draw comes on its next update, while collision
        // sees the solid block at once. Only its type changes, so it keeps what it shows until that draw (#2502).
        level.SetCollision(position, RoomCollisionType.SolidBlock);
        return true;
    }
}
