using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Coloured and blue door PLMs: their setup, shot reactions, opening and the door-closing spawn after a transition.</summary>
[RoomLifetime]
internal sealed class DoorPlms : IPlmFamilySource
{
    /// <summary>Registers the ColoredDoor family, which has no step of its own: its slots take the shared path.</summary>
    public void AddPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(PlmFamily.ColoredDoor, static (_, _) => PlmStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Allocates fallback closing and blue-door actors from the native PLM pool.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Queues the library-two response to rejected colored-door hits.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Binds door PLM logic to room persistence, drawing, allocation, and sound owners.</summary>
    internal DoorPlms(SharedGameState? system, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _system = system;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>
    /// Executes <c>$82:E8EB Spawn_Door_Closing_PLM</c> after destination room PLMs and
    /// door/setup code have been constructed.
    /// </summary>
    /// <remarks>
    /// The cartridge first looks for any resident PLM at the door-cap block. If that
    /// actor's nonnegative persistence bit is not already set, its header's second list is
    /// selected in place. Otherwise the twelve-entry direction table may spawn a separate
    /// blue-door or escape-gate closer. Allocation always uses the highest free native
    /// slot, exactly like <c>Spawn_Room_PLM</c>.
    /// </remarks>
    /// <returns>
    /// True when an existing cap was redirected or a fallback closer was spawned; false
    /// for non-closing directions or native pool exhaustion.
    /// </returns>
    public bool TrySpawnDoorClosingPlm(
        RoomLevelData level,
        DoorDefinition door,
        SharedGameState system)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(door);
        Ensure.NotNull(system);

        int blockIndex = level.GetBlockIndex(door.PlmBlock);
        foreach (PlmSlot resident in _slotTable.InProcessingOrder)
        {
            if (!resident.Active || resident.BlockIndex != blockIndex)
                continue;

            // `$82:E951-$E963` treats a negative room argument as explicitly
            // nonpersistent. C8CA uses $8000, so the escape entrance always selects its
            // closing list. Ordinary colored caps consult the same opened-door bit array.
            if (resident.Argument is DoorBitArgument { Persists: true } residentDoorBit &&
                system.HasOpenedDoorBit(residentDoorBit.DoorBit))
            {
                break;
            }

            // Header+4 is definition metadata, not active memory: a typed cap restarts at its own closing program,
            // and the other resident closers resolve their closing start by identity. Native retains every scalar
            // the resident slot owns while replacing only its instruction pointer.
            if (resident.Animation is IResidentDoorAnimation residentDoor)
                residentDoor.StartClosing();
            else
                ResidentDoorClosings.Resolve(resident.Header).StartOn(resident);
            resident.InstructionTimer = WrappingCountdown.Start(1);
            return true;
        }

        if (DoorClosingPlms.GetDefinition(door.Orientation) is not { } fallback)
            return false;

        if (_slotTable.SpawnRoomPlm() is not { } slot)
            return false;

        slot.Header = fallback.Header;
        slot.BlockIndex = blockIndex;
        fallback.Start.StartOn(slot);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        SetupDoorTransitionDeactivatedSlot(level, slot);
        return true;
    }

    /// <summary>
    /// The deactivating setup: a solid block stays solid and any other block becomes air, keeping its appearance.
    /// Despite its historical name, the routine does not delete or deactivate the PLM slot.
    /// </summary>
    internal static void SetupDoorTransitionDeactivatedSlot(RoomLevelData level, PlmSlot slot) =>
        level.SetTerrain(slot.BlockIndex, level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.Deactivated());

    /// <summary>Installs shootable-solid collision and the resident-PLM projectile behaviour at a colored cap.</summary>
    private static void ApplyColoredDoorSetup(RoomLevelData level, int blockIndex)
    {
        // The colored-door setup keeps the block's appearance, installs shootable-solid collision, and selects the
        // resident-PLM projectile trigger so subsequent projectiles find this resident PLM.
        level.SetPlmTerrain(
            blockIndex,
            level.GetPlmCollisionBlockByIndex(blockIndex).Terrain.WithCollision(RoomCollisionType.ShootableBlock));
        level.SetPlmBehavior(blockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
    }

    /// <summary>
    /// Setup <c>$84:C7B1</c> for a coloured cap: installs its shootable collision and starts its waiting program.
    /// </summary>
    internal static void SetupColoredDoorSlot(
        RoomLevelData level,
        PlmSlot slot,
        ColoredDoorColor color,
        ColoredDoorOrientation orientation)
    {
        ApplyColoredDoorSetup(level, slot.BlockIndex);
        slot.Family = new ColoredDoorPlmState(color);
        slot.StartAnimation(new ColoredDoorPlmAnimation(color, orientation, ColoredDoorProgram.Waiting));
    }

    /// <summary>
    /// The coloured cap's shot wake (<c>$84:BD26</c>, <c>$BD50</c>, <c>$BD88</c>): a hit by the colour's projectile wakes
    /// the cap at its hit program; any other hit queues the dud sound. Either way the hit is consumed.
    /// </summary>
    internal void RunColoredDoorWake(PlmSlot slot, ColoredDoorPreInstruction preInstruction)
    {
        ColoredDoorPlmState door = slot.Family as ColoredDoorPlmState
            ?? throw new InvalidOperationException("A coloured cap's wake ran without its door state.");
        if (slot.ProjectileHit is not { Family: var family })
            return;
        bool accepted = preInstruction switch
        {
            ColoredDoorPreInstruction.WakeOnPowerBomb => family == SamusProjectileFamily.PowerBomb,
            ColoredDoorPreInstruction.WakeOnSuperMissile => family == SamusProjectileFamily.SuperMissile,
            ColoredDoorPreInstruction.WakeOnMissile => family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile,
            ColoredDoorPreInstruction.None => throw new InvalidOperationException("An inert coloured cap reached its shot test."),
            _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorPreInstruction)} {preInstruction}."),
        };
        // $BD50 seeds a red cap's counter for a Super Missile: the next count passes its threshold.
        if (preInstruction == ColoredDoorPreInstruction.WakeOnMissile && family == SamusProjectileFamily.SuperMissile)
            door.HitCounter = ColoredDoorPlmProgramDefinitions.RedSuperMissileHitCounter;
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
    /// <c>$84:8A72</c>: whether this cap's door bit records it as opened before; a negative room argument never does.
    /// </summary>
    internal bool IsDoorOpened(PlmSlot slot) =>
        slot.Argument is DoorBitArgument { Persists: true } cap && RequireSystem().HasOpenedDoorBit(cap.DoorBit);

    /// <summary>
    /// <c>$84:8A91</c> at its threshold: records the door as opened, leaves the opened doors of its window as the argument and
    /// replaces the shot wake with the inert pre-instruction.
    /// </summary>
    internal void MarkDoorOpened(PlmSlot slot)
    {
        if (slot.Argument is DoorBitArgument { Persists: true } cap)
        {
            // The cap keeps the opened doors of the window it just updated, marked spent.
            SharedGameState system = RequireSystem();
            system.SetOpenedDoorBit(cap.DoorBit);
            int firstDoor = SpentDoorArgument.FirstDoorOfWindow(cap.DoorBit);
            slot.Argument = new SpentDoorArgument(firstDoor, system.OpenedDoorsFrom(firstDoor, SpentDoorArgument.WindowDoorCount));
        }
        slot.PreInstruction = PlmPreInstructions.Inert;
    }

    /// <summary><c>$84:8A91</c>: counts a hit on a coloured cap; true when it opens the door.</summary>
    internal bool CountColoredDoorHit(PlmSlot slot)
    {
        ColoredDoorPlmState door = (slot.Family as ColoredDoorPlmState)
            ?? throw new InvalidOperationException("A coloured cap's hit reached a slot without its door state.");
        door.HitCounter++;
        if (door.HitCounter < ColoredDoorPlmProgramDefinitions.HitThresholds[door.Color])
            return false;
        MarkDoorOpened(slot);
        return true;
    }

    /// <summary>The room's door-bit persistence owner.</summary>
    private SharedGameState RequireSystem() =>
        _system ?? throw new InvalidOperationException("A resident door has no persistence owner.");

    /// <summary>
    /// Names the color and facing of a colored-door PLM (#1414: each of the twelve doors is matched by member, replacing
    /// the color ranges and six-byte header stride the cartridge's header words implied).
    /// </summary>
    internal static bool TryIdentifyColoredDoor(
        PlmHeaderId header,
        out ColoredDoorColor color,
        out ColoredDoorOrientation orientation)
    {
        bool found = DoorPlmHeaderDefinitions.ColoredDoors.TryGetValue(header, out ColoredDoorVariant variant);
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

        PlmHeaderId blueDoorHeader = orientation switch
        {
            ColoredDoorOrientation.Left => PlmHeaderId.BlueDoorFacingLeft,
            ColoredDoorOrientation.Right => PlmHeaderId.BlueDoorFacingRight,
            ColoredDoorOrientation.Up => PlmHeaderId.BlueDoorFacingUp,
            ColoredDoorOrientation.Down => PlmHeaderId.BlueDoorFacingDown,
            _ => throw new InvalidOperationException(
                "A validated blue-door facing escaped its four-way header table."),
        };

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.Header = blueDoorHeader;
        slot.BlockIndex = blockIndex;
        slot.RestoreTerrain = PlmSlotDefinitions.ClearedRestoreTerrain;
        slot.StartAnimation(new BlueDoorPlmAnimation(BlueDoorPlmProgramDefinitions.OpeningPrograms[orientation]));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.PreInstruction = PlmPreInstructions.Inert;
        // The native spawn leaves the argument zero: door bit zero, which a later closing check reads.
        slot.Argument = new DoorBitArgument(0, DoorOpeningPersistence.Persistent);
        slot.LoopTimer = 0;
        slot.Family = null;

        // This is a direct terrain write, not a PLM draw. The first animated draw occurs on this actor's next handler
        // pass and owns the screen update, while collision observes the solid origin immediately.
        level.SetTerrain(blockIndex,
            level.GetCollisionBlockByIndex(blockIndex).Terrain.WithCollision(RoomCollisionType.SolidBlock));
        return true;
    }
}
