using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What the room's eye doors (Tourian and Kraid's lair) consult (#1554): their setup, whether Samus is near, the shots,
/// sweat and smoke the eye spawns, its sounds, and the move of the destroyed eye onto its blue cap. The eye
/// (<see cref="EyeDoorEyeObject"/>) and its body and bottom (<see cref="EyeDoorPartObject"/>) are typed room objects.
/// </summary>
[RoomLifetime]
internal sealed class EyeDoorObjects
{
    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects _enemies;
    /// <summary>Queues rejected-shot and launched-projectile sounds.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the eye doors to room services and sound requests.</summary>
    internal EyeDoorObjects(SharedGameState system, IRoomSamusAccess samus, IRoomObjectEnemyEffects enemies, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _enemies = enemies;
        _sounds = sounds;
    }

    /// <summary>Recognizes the eye, middle, and bottom definitions for either door orientation.</summary>
    internal static bool IsEyeDoorKind(RoomObjectKind kind) => EyeDoorPartDefinitions.Parts.ContainsKey(kind);

    /// <summary>
    /// Places the eye-door part the room's population placed as <paramref name="placed"/> in <paramref name="door"/>, which
    /// takes its first look on its first update, and installs its closed collision blocks unless its door has been opened
    /// for good.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="placed"/> is not an eye-door part.</exception>
    internal void SetupEyeDoor(
        RoomLevelData level,
        RoomObjectPlace place,
        RoomObjectKind placed,
        DoorBitArgument? door)
    {
        Ensure.NotNull(place);
        (EyeDoorComponent component, EyeDoorOrientation orientation) = EyeDoorPartDefinitions.Parts.TryGetValue(placed, out EyeDoorPart part)
            ? part
            : throw new ArgumentOutOfRangeException(nameof(placed), placed, "Not an eye-door room object.");
        PlacedDoor? placedDoor = PlacedDoor.For(door);
        place.Become(component == EyeDoorComponent.Eye
            ? new EyeDoorEyeObject(orientation, placedDoor)
            : new EyeDoorPartObject(orientation, component, placedDoor));

        if (placedDoor?.IsOpenedForGood(_system.HasOpenedDoor) == true)
        {
            // Setup leaves an opened door's terrain untouched: the part's first look removes a body or bottom, and turns
            // the eye into the ordinary blue cap.
            return;
        }

        if (component == EyeDoorComponent.Eye)
        {
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, place.Block, EyeDoorDefinitions.EyeCollision,
                EyeDoorDefinitions.EyeBehavior);
            WriteExtensionRows(level, place.Block, EyeDoorDefinitions.EyeExtensionRows);
            return;
        }

        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, place.Block, EyeDoorDefinitions.ClosedComponentCollision,
            EyeDoorDefinitions.ClosedComponentBehavior);
    }

    /// <summary>Queues the dud sound of a shot the eye shrugs off.</summary>
    internal void QueueRejectedShotSound() =>
        _sounds.Queue(EyeDoorDefinitions.RejectedShotSound, Priority: SoundRequestPriority.Normal);

    /// <summary>Whether Samus stands within <paramref name="reach"/> blocks across and up or down of the part.</summary>
    internal bool IsSamusNear(RoomObjectPlace place, Extent16<RoomBlocks> reach)
    {
        IRoomSamus samus = _samus.Current
            ?? throw new InvalidOperationException("Eye-door proximity requires Samus.");
        int roomObjectX = place.Block.X;
        int roomObjectY = place.Block.Y;
        int deltaX = Math.Abs(RoomGridConversions.PixelToBlock(samus.Position.X.Whole) - roomObjectX);
        int deltaY = Math.Abs(RoomGridConversions.PixelToBlock(samus.Position.Y.Whole) - roomObjectY);
        return deltaX <= reach.X && deltaY <= reach.Y;
    }

    /// <summary>The eye fires its projectile and queues its sound.</summary>
    internal void Shoot(RoomObjectPlace place, EyeDoorEyeObject eye)
    {
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.Projectile, eye);
        _sounds.Queue(
            EyeDoorDefinitions.ProjectileSound,
            Priority: SoundRequestPriority.Normal);
    }

    /// <summary>The eye sweats.</summary>
    internal void SpawnSweat(RoomObjectPlace place, EyeDoorEyeObject eye) =>
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.Sweat, eye);

    /// <summary>Two randomly placed smoke puffs about the eye.</summary>
    internal void SpawnTwoSmoke(RoomObjectPlace place, EyeDoorEyeObject eye)
    {
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.ScatteredSmoke, eye);
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.ScatteredSmoke, eye);
    }

    /// <summary>One centred smoke puff about the eye.</summary>
    internal void SpawnSmoke(RoomObjectPlace place, EyeDoorEyeObject eye) =>
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.CenteredSmoke, eye);

    /// <summary>
    /// Moves the eye up one block and writes the blue cap facing
    /// <paramref name="orientation"/> with its extension rows.
    /// </summary>
    internal static void ConvertEyeToBlueDoor(RoomLevelData level, RoomObjectPlace place, EyeDoorOrientation orientation)
    {
        RoomBlockBehavior facing = orientation switch
        {
            EyeDoorOrientation.Left => RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Left)),
            EyeDoorOrientation.Right => RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right)),
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
        };
        place.Block += new Offset16<RoomBlocks>(0, -1);
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
            level,
            place.Block,
            RoomCollisionType.ShootableBlock,
            facing);
        WriteExtensionRows(level, place.Block, EyeDoorDefinitions.BlueDoorExtensionCount);
    }

    /// <summary>
    /// Writes <paramref name="rows"/> vertical extension blocks below <paramref name="origin"/>, each pointing back
    /// up to the origin block.
    /// </summary>
    private static void WriteExtensionRows(RoomLevelData level, Position16<RoomBlocks> origin, int rows)
    {
        for (int row = 1; row <= rows; row++)
        {
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
                level,
                origin + new Offset16<RoomBlocks>(0, row),
                RoomCollisionType.VerticalExtension,
                RoomBlockBehavior.Extending(-row));
        }
    }

    /// <summary>
    /// Sends an eye-door projectile request with the eye's facing, source block and door, none once the eye is spent, to
    /// enemy services.
    /// </summary>
    private void SpawnEyeDoorProjectile(
        RoomObjectPlace place,
        EyeDoorProjectileDefinition definition,
        EyeDoorEyeObject eye)
    {
        _enemies.SpawnEyeDoorProjectile(new EyeDoorProjectileRequest(
            definition,
            eye.Orientation,
            place.Block,
            eye.Door?.Door));
    }
}
