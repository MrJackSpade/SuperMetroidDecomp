using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Coloured and blue door PLMs: their setup, shot reactions, opening and the door-closing spawn after a transition.</summary>
[RoomLifetime]
internal sealed class DoorObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the ColoredDoor family, which has no step of its own: its slots take the shared path.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.ColoredDoor, static (_, _) => RoomObjectStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Allocates fallback closing and blue-door actors from the PLM pool.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>Queues the library-two response to rejected colored-door hits.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds door PLM logic to room persistence, drawing, allocation, and sound owners.</summary>
    internal DoorObjects(SharedGameState? system, RoomObjectSlotTable slotTable, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>
    /// Spawns the closing PLM for the door Samus entered through, after destination room PLMs and
    /// door/setup code have been constructed.
    /// </summary>
    /// <remarks>
    /// The game first looks for any resident PLM at the door-cap block. If that
    /// actor's nonnegative persistence bit is not already set, its header's second list is
    /// selected in place. Otherwise the twelve-entry direction table may spawn a separate
    /// blue-door or escape-gate closer. Allocation always uses the highest free
    /// slot, exactly like an ordinary room PLM spawn.
    /// </remarks>
    /// <returns>
    /// True when an existing cap was redirected or a fallback closer was spawned; false
    /// for non-closing directions or an exhausted PLM pool.
    /// </returns>
    public bool TrySpawnDoorClosingObject(
        RoomLevelData level,
        DoorDefinition door,
        SharedGameState system)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(door);
        Ensure.NotNull(system);

        int blockIndex = level.GetBlockIndex(door.RoomObjectBlock);
        foreach (RoomObjectSlot resident in _slotTable.InProcessingOrder)
        {
            if (!resident.Active || resident.BlockIndex != blockIndex)
                continue;

            // A negative room argument is explicitly
            // nonpersistent. The escape entrance's argument is negative, so it always selects its
            // closing list. Ordinary colored caps consult the same opened-door bit array.
            if (resident.Argument is DoorBitArgument { Persists: true } residentDoorBit &&
                system.HasOpenedDoorBit(residentDoorBit.DoorBit))
            {
                break;
            }

            // Header+4 is definition metadata, not active memory: a typed cap restarts at its own closing program,
            // and the other resident closers resolve their closing start by identity. The game retains every scalar
            // the resident slot owns while replacing only its animation script.
            if (resident.Animation is IResidentDoorAnimation residentDoor)
                residentDoor.StartClosing();
            else
                ResidentDoorClosings.Resolve(resident.Header).StartOn(resident);
            resident.FrameHold = 1;
            return true;
        }

        if (DoorClosingObjects.GetDefinition(door.Orientation) is not { } fallback)
            return false;

        if (_slotTable.SpawnRoomObject() is not { } slot)
            return false;

        slot.Header = fallback.Header;
        slot.BlockIndex = blockIndex;
        fallback.Start.StartOn(slot);
        slot.FrameHold = 1;
        SetupDoorTransitionDeactivatedSlot(level, slot);
        return true;
    }

    /// <summary>
    /// The deactivating setup: a solid block stays solid and any other block becomes air, keeping its appearance.
    /// Despite its historical name, the routine does not delete or deactivate the PLM slot.
    /// </summary>
    internal static void SetupDoorTransitionDeactivatedSlot(RoomLevelData level, RoomObjectSlot slot) =>
        level.SetTerrain(slot.BlockIndex, level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.Deactivated());

    /// <summary>Installs shootable-solid collision and the resident-PLM projectile behaviour at a colored cap.</summary>
    private static void ApplyColoredDoorSetup(RoomLevelData level, int blockIndex)
    {
        // The colored-door setup keeps the block's appearance, installs shootable-solid collision, and selects the
        // resident-PLM projectile trigger so subsequent projectiles find this resident PLM.
        level.SetObjectTerrain(
            blockIndex,
            level.GetObjectCollisionBlockByIndex(blockIndex).Terrain.WithCollision(RoomCollisionType.ShootableBlock));
        level.SetObjectBehavior(blockIndex, RoomBlockBehaviorValues.ResidentObjectProjectileTrigger);
    }

    /// <summary>
    /// Sets up a coloured cap: installs its shootable collision and starts its waiting program.
    /// </summary>
    internal static void SetupColoredDoorSlot(
        RoomLevelData level,
        RoomObjectSlot slot,
        ColoredDoorColor color,
        ColoredDoorOrientation orientation)
    {
        ApplyColoredDoorSetup(level, slot.BlockIndex);
        slot.Family = new ColoredDoorObjectState(color);
        slot.StartAnimation(new ColoredDoorObjectAnimation(color, orientation, ColoredDoorProgram.Waiting));
    }

    /// <summary>
    /// The coloured cap's shot wake: a hit by the colour's projectile wakes
    /// the cap at its hit program; any other hit queues the dud sound. Either way the hit is consumed.
    /// </summary>
    internal void RunColoredDoorWake(RoomObjectSlot slot, ColoredDoorFrameBehavior frameBehavior)
    {
        ColoredDoorObjectState door = slot.Family as ColoredDoorObjectState
            ?? throw new InvalidOperationException("A coloured cap's wake ran without its door state.");
        if (slot.ProjectileHit is not { Family: var family })
            return;
        bool accepted = frameBehavior switch
        {
            ColoredDoorFrameBehavior.WakeOnPowerBomb => family == SamusProjectileFamily.PowerBomb,
            ColoredDoorFrameBehavior.WakeOnSuperMissile => family == SamusProjectileFamily.SuperMissile,
            ColoredDoorFrameBehavior.WakeOnMissile => family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile,
            ColoredDoorFrameBehavior.None => throw new InvalidOperationException("An inert coloured cap reached its shot test."),
            _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorFrameBehavior)} {frameBehavior}."),
        };
        // The Super Missile frame behaviour seeds a red cap's counter: the next count passes its threshold.
        if (frameBehavior == ColoredDoorFrameBehavior.WakeOnMissile && family == SamusProjectileFamily.SuperMissile)
            door.HitCounter = ColoredDoorObjectProgramDefinitions.RedSuperMissileHitCounter;
        if (accepted)
        {
            slot.ProjectileHit = null;
            slot.WakeAtLink();
            return;
        }
        QueueDudSound();
        slot.ProjectileHit = null;
    }

    /// <summary>Queues the library-two dud sound a rejected or premature shot makes.</summary>
    internal void QueueDudSound() =>
        _sounds.Queue(Library2Sound.DudShotOnDoor, Priority: DudSoundPriority);

    /// <summary>The dud sound's request priority.</summary>
    private static readonly SoundRequestPriority DudSoundPriority = SoundRequestPriority.Normal;

    /// <summary>
    /// Whether this cap's door bit records it as opened before; a negative room argument never does.
    /// </summary>
    internal bool IsDoorOpened(RoomObjectSlot slot) =>
        slot.Argument is DoorBitArgument { Persists: true } cap && RequireSystem().HasOpenedDoorBit(cap.DoorBit);

    /// <summary>
    /// The opened-door step at its threshold: records the door as opened, leaves the opened doors of its window as the argument and
    /// replaces the shot wake with the inert frame behaviour.
    /// </summary>
    internal void MarkDoorOpened(RoomObjectSlot slot)
    {
        if (slot.Argument is DoorBitArgument { Persists: true } cap)
        {
            // The cap keeps the opened doors of the window it just updated, marked spent.
            SharedGameState system = RequireSystem();
            system.SetOpenedDoorBit(cap.DoorBit);
            int firstDoor = SpentDoorArgument.FirstDoorOfWindow(cap.DoorBit);
            slot.Argument = new SpentDoorArgument(firstDoor, system.OpenedDoorsFrom(firstDoor, SpentDoorArgument.WindowDoorCount));
        }
        slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
    }

    /// <summary>Counts a hit on a coloured cap; true when it opens the door.</summary>
    internal bool CountColoredDoorHit(RoomObjectSlot slot)
    {
        ColoredDoorObjectState door = (slot.Family as ColoredDoorObjectState)
            ?? throw new InvalidOperationException("A coloured cap's hit reached a slot without its door state.");
        door.HitCounter++;
        if (door.HitCounter < ColoredDoorObjectProgramDefinitions.HitThresholds[door.Color])
            return false;
        MarkDoorOpened(slot);
        return true;
    }

    /// <summary>The room's door-bit persistence owner.</summary>
    private SharedGameState RequireSystem() =>
        _system ?? throw new InvalidOperationException("A resident door has no persistence owner.");

    /// <summary>
    /// Names the color and facing of a colored-door PLM (#1414: each of the twelve doors is matched by member, replacing
    /// the color ranges and six-byte header stride the PLM headers implied).
    /// </summary>
    internal static bool TryIdentifyColoredDoor(
        RoomObjectHeaderId header,
        out ColoredDoorColor color,
        out ColoredDoorOrientation orientation)
    {
        bool found = DoorObjectHeaderDefinitions.ColoredDoors.TryGetValue(header, out ColoredDoorVariant variant);
        color = variant.Color;
        orientation = variant.Orientation;
        return found;
    }

    /// <summary>
    /// Spawns the blue-door opening selected by a shootable block's blue-door behaviour and applies its setup.
    /// </summary>
    /// <remarks>
    /// The cap's top/left origin begins as a shootable solid. Setup changes only that origin to a plain solid,
    /// preserving its appearance; the opening program then animates all four cap blocks and ends on the ordinary
    /// air-door frame.
    /// A power bomb is the one rejected projectile family. Exhausting all forty PLM slots
    /// silently loses the request, matching <c>Spawn_PLM_to_CurrentBlockIndex</c>.
    /// </remarks>
    public bool TrySpawnBlueDoorOpening(
        RoomLevelData level,
        int blockIndex,
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

        // A power-bomb collision deletes the just-allocated PLM without touching the cap.
        if (projectileType.Family == SamusProjectileFamily.PowerBomb)
            return false;

        RoomObjectHeaderId blueDoorHeader = orientation switch
        {
            ColoredDoorOrientation.Left => RoomObjectHeaderId.BlueDoorFacingLeft,
            ColoredDoorOrientation.Right => RoomObjectHeaderId.BlueDoorFacingRight,
            ColoredDoorOrientation.Up => RoomObjectHeaderId.BlueDoorFacingUp,
            ColoredDoorOrientation.Down => RoomObjectHeaderId.BlueDoorFacingDown,
            _ => throw new InvalidOperationException(
                "A validated blue-door facing escaped its four-way header table."),
        };

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.Header = blueDoorHeader;
        slot.BlockIndex = blockIndex;
        slot.RestoreTerrain = RoomObjectSlotDefinitions.ClearedRestoreTerrain;
        slot.StartAnimation(new BlueDoorObjectAnimation(BlueDoorObjectProgramDefinitions.OpeningPrograms[orientation]));
        slot.FrameHold = 1;
        slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
        // The spawn leaves the argument zero: door bit zero, which a later closing check reads.
        slot.Argument = new DoorBitArgument(0, DoorOpeningPersistence.Persistent);
        slot.LoopTimer = WrappingCountdown.Start(0);
        slot.Family = null;

        // This is a direct terrain write, not a PLM draw. The first animated draw occurs on this actor's next handler
        // pass and owns the screen update, while collision observes the solid origin immediately.
        level.SetTerrain(blockIndex,
            level.GetCollisionBlockByIndex(blockIndex).Terrain.WithCollision(RoomCollisionType.SolidBlock));
        return true;
    }
}
