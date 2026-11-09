using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Eye-door PLMs (Tourian and Kraid lair): eye projectile spawning, hit reaction and conversion to a blue door.</summary>
[RoomLifetime]
internal sealed class EyeDoorObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the EyeDoor family, which has no step of its own: its slots take the shared path.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.EyeDoor, static (_, _) => RoomObjectStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;
    /// <summary>Queues rejected-shot and launched-projectile sounds.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds eye-door PLM handling to room services and sound requests.</summary>
    internal EyeDoorObjects(SharedGameState? system, IRoomSamusAccess? samus, IRoomObjectEnemyEffects? enemies, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _enemies = enemies;
        _sounds = sounds;
    }

    /// <summary>Recognizes the eye, middle, and bottom headers for either door orientation.</summary>
    internal static bool IsEyeDoorHeader(RoomObjectHeaderId header) => EyeDoorPartDefinitions.Parts.ContainsKey(header);

    /// <summary>Initializes a door component and its closed collision blocks unless its persistent bit is set.</summary>
    internal void SetupEyeDoorSlot(
        RoomLevelData level,
        RoomObjectSlot slot)
    {
        (EyeDoorComponent component, EyeDoorOrientation orientation) = EyeDoorPartDefinitions.Parts.TryGetValue(slot.Header, out EyeDoorPart part)
            ? part
            : throw new ArgumentOutOfRangeException(nameof(slot), slot.Header, "Not an eye-door room object.");
        slot.Family = new EyeDoorObjectState(component);
        slot.StartAnimation(new EyeDoorObjectAnimation(orientation, component,
            component == EyeDoorComponent.Eye ? EyeDoorObjectProgram.EyeStart : EyeDoorObjectProgram.ComponentStart));

        SharedGameState system = _system ??
            throw new InvalidOperationException("Eye-door setup has no persistence owner.");
        if (slot.Argument is DoorBitArgument { Persists: true } openedEye &&
            system.HasOpenedDoor(openedEye.Door))
        {
            // Setup routines deliberately leave already-open terrain untouched. Their first
            // bytecode instruction takes the persistent branch and either deletes this
            // passive component or constructs the ordinary blue-door cap from the eye.
            return;
        }

        if (component == EyeDoorComponent.Eye)
        {
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.BlockIndex, EyeDoorDefinitions.EyeCollision,
                EyeDoorDefinitions.EyeBehavior);
            WriteExtensionRows(level, slot.BlockIndex, EyeDoorDefinitions.EyeExtensionRows);
            return;
        }

        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.BlockIndex, EyeDoorDefinitions.ClosedComponentCollision,
            EyeDoorDefinitions.ClosedComponentBehavior);
    }

    /// <summary>Wakes opened components or consumes a pending missile hit before instruction dispatch.</summary>
    internal void RunEyeDoorFrameBehavior(RoomObjectSlot slot, EyeDoorFrameBehavior frameBehavior)
    {
        EyeDoorObjectState eyeDoor = slot.Family as EyeDoorObjectState
            ?? throw new InvalidOperationException("An eye door's wake ran without its component state.");
        SharedGameState system = _system ??
            throw new InvalidOperationException("A live eye door has no persistence owner.");
        switch (frameBehavior)
        {
            case EyeDoorFrameBehavior.WakeWhenDoorBitSet:
                if (slot.Argument is DoorBitArgument { Persists: true } wakingEye &&
                    system.HasOpenedDoor(wakingEye.Door))
                {
                    slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
                    slot.WakeAtLink();
                }
                return;

            case EyeDoorFrameBehavior.MissileHit:
                if (!eyeDoor.HasPendingHit)
                    return;

                SamusProjectileFamily? family = slot.ProjectileHit?.Family;
                slot.ProjectileHit = null;
                eyeDoor.HasPendingHit = false;
                if (family == SamusProjectileFamily.SuperMissile)
                {
                    // The Super Missile frame behaviour seeds the counter so the next increment meets the threshold.
                    eyeDoor.HitCounter = EyeDoorObjectProgramDefinitions.SuperMissileHitCounter;
                }
                else if (family != SamusProjectileFamily.Missile)
                {
                    _sounds.Queue(
                        EyeDoorDefinitions.RejectedShotSound,
                        Priority: SoundRequestPriority.Normal);
                    return;
                }

                slot.WakeAtLink();
                return;

            default:
                throw new InvalidOperationException($"Undefined EyeDoorFrameBehavior {frameBehavior}.");
        }
    }

    /// <summary>Whether Samus stands within <paramref name="reach"/> blocks across and up or down of the PLM.</summary>
    internal bool IsSamusNear(RoomLevelData level, RoomObjectSlot slot, Extent16<RoomBlocks> reach)
    {
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Eye-door proximity requires Samus.");
        int roomObjectX = slot.BlockIndex % level.Size.X;
        int roomObjectY = slot.BlockIndex / level.Size.X;
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

    /// <summary>Counts a hit on the eye; true when it opens the door.</summary>
    internal static bool CountEyeHit(RoomObjectSlot slot, DoorObjects doors)
    {
        EyeDoorObjectState eye = (slot.Family as EyeDoorObjectState)
            ?? throw new InvalidOperationException("An eye's hit reached a slot without its eye-door state.");
        eye.HitCounter++;
        if (eye.HitCounter < EyeDoorObjectProgramDefinitions.HitThreshold)
            return false;
        doors.MarkDoorOpened(slot);
        return true;
    }

    /// <summary>
    /// Moves the eye PLM up one block and writes the blue cap facing
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
        slot.BlockIndex = checked(slot.BlockIndex - level.Size.X);
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
            level,
            slot.BlockIndex,
            RoomCollisionType.ShootableBlock,
            facing);
        WriteExtensionRows(level, slot.BlockIndex, EyeDoorDefinitions.BlueDoorExtensionCount);
    }

    /// <summary>
    /// Writes <paramref name="rows"/> vertical extension blocks below <paramref name="originIndex"/>, each pointing back
    /// up to the origin block.
    /// </summary>
    private static void WriteExtensionRows(RoomLevelData level, int originIndex, int rows)
    {
        for (int row = 1; row <= rows; row++)
        {
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
                level,
                checked(originIndex + row * level.Size.X),
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
            slot.BlockIndex,
            (slot.Argument as DoorBitArgument)?.Door));
    }

    /// <summary>Per-component hit state retained across eye-door instruction frames.</summary>
    internal sealed class EyeDoorObjectState(EyeDoorComponent component) : RoomObjectFamilyState, IShotTriggeredObject
    {
        /// <summary>Only the eye takes the shot; the middle and bottom components ignore it.</summary>
        public void RecordShot()
        {
            if (Component == EyeDoorComponent.Eye)
                HasPendingHit = true;
        }

        /// <inheritdoc/>
        internal override RoomObjectFamily Family => RoomObjectFamily.EyeDoor;

        /// <summary>Identifies whether this slot is the eye, middle, or bottom component.</summary>
        public EyeDoorComponent Component { get; } = component;
        /// <summary>Missile-hit count used by the door's threshold step. It only counts up, by one per hit or to 120 on a Super Missile hit, and the door opens once it reaches its threshold, so it stays far below the 255 the original byte counter could hold.</summary>
        public int HitCounter { get; set; }
        /// <summary>Whether a projectile collision awaits the eye's missile pre-instruction.</summary>
        public bool HasPendingHit { get; set; }
    }
}
