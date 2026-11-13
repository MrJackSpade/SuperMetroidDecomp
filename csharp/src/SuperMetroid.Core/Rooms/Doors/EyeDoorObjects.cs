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
    private readonly SharedGameState? _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;
    /// <summary>Queues rejected-shot and launched-projectile sounds.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the eye doors to room services and sound requests.</summary>
    internal EyeDoorObjects(SharedGameState? system, IRoomSamusAccess? samus, IRoomObjectEnemyEffects? enemies, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _enemies = enemies;
        _sounds = sounds;
    }

    /// <summary>Recognizes the eye, middle, and bottom headers for either door orientation.</summary>
    internal static bool IsEyeDoorHeader(RoomObjectHeaderId header) => EyeDoorPartDefinitions.Parts.ContainsKey(header);

    /// <summary>
    /// Places the eye-door part the room's population placed as <paramref name="placed"/>, which takes its first look on
    /// its first update, and installs its closed collision blocks unless its door has been opened for good.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="placed"/> is not an eye-door part.</exception>
    internal void SetupEyeDoorSlot(
        RoomLevelData level,
        RoomObjectSlot slot,
        RoomObjectHeaderId placed)
    {
        Ensure.NotNull(slot);
        (EyeDoorComponent component, EyeDoorOrientation orientation) = EyeDoorPartDefinitions.Parts.TryGetValue(placed, out EyeDoorPart part)
            ? part
            : throw new ArgumentOutOfRangeException(nameof(placed), placed, "Not an eye-door room object.");
        slot.Become(component == EyeDoorComponent.Eye
            ? new EyeDoorEyeObject(orientation)
            : new EyeDoorPartObject(orientation, component));

        SharedGameState system = _system ??
            throw new InvalidOperationException("Eye-door setup has no persistence owner.");
        if (slot.Argument is DoorBitArgument { Persists: true } openedEye &&
            system.HasOpenedDoor(openedEye.Door))
        {
            // Setup leaves an opened door's terrain untouched: the part's first look removes a body or bottom, and turns
            // the eye into the ordinary blue cap.
            return;
        }

        if (component == EyeDoorComponent.Eye)
        {
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.Block, EyeDoorDefinitions.EyeCollision,
                EyeDoorDefinitions.EyeBehavior);
            WriteExtensionRows(level, slot.Block, EyeDoorDefinitions.EyeExtensionRows);
            return;
        }

        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.Block, EyeDoorDefinitions.ClosedComponentCollision,
            EyeDoorDefinitions.ClosedComponentBehavior);
    }

    /// <summary>Queues the dud sound of a shot the eye shrugs off.</summary>
    internal void QueueRejectedShotSound() =>
        _sounds.Queue(EyeDoorDefinitions.RejectedShotSound, Priority: SoundRequestPriority.Normal);

    /// <summary>Whether Samus stands within <paramref name="reach"/> blocks across and up or down of the part.</summary>
    internal bool IsSamusNear(RoomObjectSlot slot, Extent16<RoomBlocks> reach)
    {
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Eye-door proximity requires Samus.");
        int roomObjectX = slot.Block.X;
        int roomObjectY = slot.Block.Y;
        int deltaX = Math.Abs((samus.Position.X.Whole >> 4) - roomObjectX);
        int deltaY = Math.Abs((samus.Position.Y.Whole >> 4) - roomObjectY);
        return deltaX <= reach.X && deltaY <= reach.Y;
    }

    /// <summary>The eye facing <paramref name="facing"/> fires its projectile and queues its sound.</summary>
    internal void Shoot(RoomObjectSlot slot, EyeDoorOrientation facing)
    {
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Projectile, facing);
        _sounds.Queue(
            EyeDoorDefinitions.ProjectileSound,
            Priority: SoundRequestPriority.Normal);
    }

    /// <summary>The eye facing <paramref name="facing"/> sweats.</summary>
    internal void SpawnSweat(RoomObjectSlot slot, EyeDoorOrientation facing) =>
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Sweat, facing);

    /// <summary>Two randomly placed smoke puffs.</summary>
    internal void SpawnTwoSmoke(RoomObjectSlot slot, EyeDoorOrientation facing)
    {
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.ScatteredSmoke, facing);
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.ScatteredSmoke, facing);
    }

    /// <summary>One centred smoke puff.</summary>
    internal void SpawnSmoke(RoomObjectSlot slot, EyeDoorOrientation facing) =>
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.CenteredSmoke, facing);

    /// <summary>
    /// Moves the eye up one block and writes the blue cap facing
    /// <paramref name="orientation"/> with its extension rows.
    /// </summary>
    internal static void ConvertEyeToBlueDoor(RoomLevelData level, RoomObjectSlot slot, EyeDoorOrientation orientation)
    {
        RoomBlockBehavior facing = orientation switch
        {
            EyeDoorOrientation.Left => RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Left)),
            EyeDoorOrientation.Right => RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right)),
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
        };
        slot.Block += new Offset16<RoomBlocks>(0, -1);
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
            level,
            slot.Block,
            RoomCollisionType.ShootableBlock,
            facing);
        WriteExtensionRows(level, slot.Block, EyeDoorDefinitions.BlueDoorExtensionCount);
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

    /// <summary>Sends an eye-door projectile request with the eye's facing, source block and room argument to enemy services.</summary>
    private void SpawnEyeDoorProjectile(
        RoomObjectSlot slot,
        EyeDoorProjectileDefinition definition,
        EyeDoorOrientation facing)
    {
        IEyeDoorProjectileSpawner enemies = _enemies ??
            throw new InvalidOperationException("Eye-door program has no enemy-projectile spawner.");
        enemies.SpawnEyeDoorProjectile(new EyeDoorProjectileRequest(
            definition,
            facing,
            slot.Block,
            (slot.Argument as DoorBitArgument)?.Door));
    }
}
