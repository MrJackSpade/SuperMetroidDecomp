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
    /// Closes the door Samus entered through behind her, after the destination room's objects and its door and setup
    /// code have been placed.
    /// </summary>
    /// <remarks>
    /// The first object at the door's block, in the room's update order, closes unless it stays open
    /// (<see cref="IDoorKeptOpen"/>, #2091): a cap or eye door whose door has been opened for good stays open, and so does a
    /// blue cap a shot opened while the room's <see cref="ShotOpenedCapClosing{TDoor}"/> rule keeps it open (#1906).
    /// Otherwise a coloured or grey cap, an eye door's body or Mother Brain's escape gate closes from its next update.
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
        foreach (RoomObjectPlace resident in _places.InUpdateOrder)
        {
            if (!resident.Active || resident.Block != block)
                continue;

            // A cap whose opening never persists always closes. The escape entrance's cap never persists, so it
            // always selects its closing sequence. Ordinary coloured caps consult the doors opened for good.
            if (resident.Behaviour is IDoorKeptOpen kept && kept.StaysOpenBehindSamus(system.HasOpenedDoor, _shotCapClosing))
                break;

            // The closing keeps everything else the object holds: a cap that waits for a shot still does.
            (resident.Behaviour as IClosingDoor ?? throw new InvalidDataException(
                $"The room object at the door's block {block} has no closing behind Samus.")).CloseBehindSamus();
            return true;
        }

        if (DoorClosingObjects.GetDefinition(door.Orientation) is not { } fallback)
            return false;

        if (_places.Spawn() is not { } place)
            return false;

        place.Block = block;
        place.Become(fallback.BlueCapFacing is { } facing
            ? new BlueDoorCapObject(facing, BlueDoorCapPhase.Closing)
            : new MotherBrainEscapeGateObject(MotherBrainEscapeGateDefinitions.Closing));
        SetupPassableBlock(level, place);
        return true;
    }

    /// <summary>
    /// The setup that makes a room object's block passable: a solid block stays solid and any other block becomes air,
    /// keeping its appearance. The room object keeps its place.
    /// </summary>
    internal static void SetupPassableBlock(RoomLevelData level, RoomObjectPlace place) =>
        level.SetTerrain(place.Block, level.GetCollisionBlock(place.Block).Terrain.Deactivated());

    /// <summary>Installs shootable-solid collision and the resident room-object projectile behaviour at a colored cap.</summary>
    private static void ApplyColoredDoorSetup(RoomLevelData level, Position16<RoomBlocks> block)
    {
        // The colored-door setup keeps the block's appearance, installs shootable-solid collision, and selects the
        // resident room-object projectile trigger so subsequent projectiles find this resident room object.
        level.SetObjectTerrain(
            block,
            level.GetObjectCollisionBlock(block).Terrain.WithCollision(RoomCollisionType.ShootableBlock));
        level.SetObjectBehavior(block, RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger));
    }

    /// <summary>
    /// Sets up a coloured cap: installs its shootable collision and places the cap, which takes its first look on its
    /// first update.
    /// </summary>
    internal static void SetupColoredDoor(
        RoomLevelData level,
        RoomObjectPlace place,
        ColoredDoorColor color,
        ColoredDoorOrientation orientation,
        LockedDoorArgument? door)
    {
        ApplyColoredDoorSetup(level, place.Block);
        place.Become(new ColoredDoorObject(new ColoredDoorVariant(color, orientation), PlacedDoor.For(door)));
    }

    /// <summary>Queues the dud sound a shot a cap does not take makes.</summary>
    internal void QueueDudSound() =>
        _sounds.Queue(EnemySound.DudShot, Priority: DudSoundPriority);

    /// <summary>The dud sound's request priority.</summary>
    private static readonly SoundRequestPriority DudSoundPriority = SoundRequestPriority.Normal;

    /// <summary>
    /// Whether <paramref name="door"/> has been opened for good before; a door whose opening never persists never has, nor
    /// has a spent object's, nor an object's placed with none.
    /// </summary>
    internal bool IsDoorOpened(PlacedDoor? door) => door?.IsOpenedForGood(_system.HasOpenedDoor) == true;

    /// <summary>
    /// Records <paramref name="door"/> opened for good, when its opening persists, which spends its object.
    /// </summary>
    internal void RecordDoorOpened(PlacedDoor? door) => door?.RecordOpened(_system);

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

        if (_places.Spawn() is not { } place)
            return false;

        place.Block = position;
        // A later closing behind Samus at this block asks the room's shot-cap rule whether the cap stays open (#1906).
        place.Become(new BlueDoorCapObject(orientation, BlueDoorCapPhase.Opening));

        // This is a direct terrain write, not a cap draw. The cap's first draw comes on its next update, while collision
        // sees the solid block at once.
        level.SetTerrain(position,
            level.GetCollisionBlock(position).Terrain.WithCollision(RoomCollisionType.SolidBlock));
        return true;
    }
}
