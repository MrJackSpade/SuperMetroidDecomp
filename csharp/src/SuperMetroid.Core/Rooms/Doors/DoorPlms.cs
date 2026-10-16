using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Coloured and blue door PLMs: their setup, shot reactions, opening and the door-closing spawn after a transition.</summary>
internal sealed class DoorPlms : IPlmFamilySource
{
    /// <summary>Registers the ColoredDoor family, which has no step of its own: its slots take the shared path.</summary>
    public void RegisterPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Register(PlmFamily.ColoredDoor, static (_, _) => PlmStepOutcome.Shared);
    }

    /// <summary>Supplies the room's door-bit persistence owner.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Draws closed and flashing door caps into level data and the background streamer.</summary>
    private readonly PlmBlockDrawing _drawing;
    /// <summary>Allocates fallback closing and blue-door actors from the native PLM pool.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Queues the library-two response to rejected colored-door hits.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Binds door PLM logic to room persistence, drawing, allocation, and sound owners.</summary>
    internal DoorPlms(PlmRoomBindings bindings, PlmBlockDrawing drawing, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _drawing = drawing;
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
        CartridgeDoorHeader door,
        Bank80SystemState system)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(door);
        Ensure.NotNull(system);

        int blockIndex = level.GetBlockIndex(door.PlmBlock);
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot resident = _slotTable._slots[slotIndex];
            if (!resident.Active || resident.BlockIndex != blockIndex)
                continue;

            // `$82:E951-$E963` treats a negative room argument as explicitly
            // nonpersistent. C8CA uses $8000, so the escape entrance always selects its
            // closing list. Ordinary colored caps consult the same opened-door bit array.
            if (unchecked((short)resident.RoomArgument) >= 0 &&
                system.HasOpenedDoorBit(resident.RoomArgument))
            {
                break;
            }

            // Header+4 is definition metadata, not active memory: a typed cap restarts at its own closing program,
            // and the other resident closers resolve their closing start by identity. Native retains every scalar
            // the resident slot owns while replacing only its instruction pointer.
            if (resident.Animation is IResidentDoorAnimation residentDoor)
                residentDoor.StartClosing();
            else
                ResidentDoorClosingDefinitions.Resolve(resident.HeaderPointer).StartOn(resident);
            resident.InstructionTimer = NativeCountdown.Start(1);
            return true;
        }

        if (DoorClosingPlmRomData.GetDefinition(door.Orientation) is not { } fallback)
            return false;

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            slot.Clear();
            slot.Active = true;
            slot.HeaderPointer = fallback.Header;
            slot.BlockIndex = blockIndex;
            fallback.Start.StartOn(slot);
            slot.InstructionTimer = NativeCountdown.Start(1);
            SetupDoorTransitionDeactivatedSlot(level, slot);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Translates <c>$84:B3C1 Setup_DeactivatePLM</c>: clear collision bits 12-14 while
    /// retaining the block's solid bit and visual index. Despite its historical name, the
    /// routine does not delete or deactivate the PLM slot.
    /// </summary>
    internal static void SetupDoorTransitionDeactivatedSlot(RoomLevelData level, PlmSlot slot)
    {
        var levelWord = new RoomLevelWord(
            level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord);
        RoomCollisionType deactivatedType = levelWord.CollisionTypeValue >=
            (byte)RoomCollisionType.SolidBlock
                ? RoomCollisionType.SolidBlock
                : RoomCollisionType.Air;
        level.SetForegroundEntry(
            slot.BlockIndex,
            (ushort)levelWord.WithCollisionType(deactivatedType));
    }

    /// <summary>Installs shootable-solid collision and resident-PLM projectile BTS at a colored cap.</summary>
    private static void ApplyColoredDoorSetup(RoomLevelData level, int blockIndex)
    {
        // Setup `$84:C7B1` preserves the visual twelve bits, installs shootable-solid
        // collision, and selects BTS $44 so subsequent projectiles find this resident PLM.
        ushort originalWord = level.GetPlmCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetPlmForegroundEntry(
            blockIndex,
            new RoomLevelWord(originalWord).WithCollisionType(RoomCollisionType.ShootableBlock).Raw);
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
    /// the cap at its hit program; any other hit queues the dud sound. Either way the hit word is consumed.
    /// </summary>
    internal void RunColoredDoorWake(PlmSlot slot, ColoredDoorPreInstruction preInstruction)
    {
        ColoredDoorPlmState door = slot.Family as ColoredDoorPlmState
            ?? throw new InvalidOperationException("A coloured cap's wake ran without its door state.");
        if (slot.LoopTimer == 0)
            return;
        SamusProjectileFamily family = new SamusProjectileTypeWord(slot.LoopTimer).Family;
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
            slot.LoopTimer = 0;
            slot.WakeAtLink();
            return;
        }
        QueueDudSound();
        slot.LoopTimer = 0;
    }

    /// <summary>Queues the library-two dud sound a rejected or premature shot makes.</summary>
    internal void QueueDudSound() =>
        _sounds._soundRequests.Add(_sounds.CreateSoundRequest(SoundEffectLibrary2Sounds.DoorOpening, MaximumQueued: DudSoundMaximumQueued));

    /// <summary>The dud sound's native queue cap.</summary>
    private const int DudSoundMaximumQueued = 6;

    /// <summary>The room-argument bit that marks a cap as opened for good: a negative argument never persists.</summary>
    private const ushort NonPersistentArgumentBit = 1 << 15;

    /// <summary>
    /// <c>$84:8A72</c>: whether this cap's door bit records it as opened before; a negative room argument never does.
    /// </summary>
    internal bool IsDoorOpened(PlmSlot slot) =>
        unchecked((short)slot.RoomArgument) >= 0 && RequireSystem().HasOpenedDoorBit(slot.RoomArgument);

    /// <summary>
    /// <c>$84:8A91</c> at its threshold: records the door bit, leaves the spent door-bit word as the argument and
    /// replaces the shot wake with the inert pre-instruction.
    /// </summary>
    internal void MarkDoorOpened(PlmSlot slot)
    {
        if (unchecked((short)slot.RoomArgument) >= 0)
        {
            // The native routine leaves the door-bit word it just updated in the argument, marked spent.
            Bank80SystemState system = RequireSystem();
            system.SetOpenedDoorBit(slot.RoomArgument);
            slot.RoomArgument = (ushort)(NonPersistentArgumentBit | system.GetOpenedDoorWordRaw(slot.RoomArgument >> 3));
        }
        slot.PreInstruction = PlmPreInstructions.Inert;
    }

    /// <summary><c>$84:8A91</c>: counts a hit on a coloured cap; true when it opens the door.</summary>
    internal bool CountColoredDoorHit(PlmSlot slot)
    {
        ColoredDoorPlmState door = (slot.Family as ColoredDoorPlmState)
            ?? throw new InvalidOperationException("A coloured cap's hit reached a slot without its door state.");
        door.HitCounter = unchecked((byte)(door.HitCounter + 1));
        if (door.HitCounter < ColoredDoorPlmProgramDefinitions.HitThresholdOf(door.Color))
            return false;
        MarkDoorOpened(slot);
        return true;
    }

    /// <summary>The room's door-bit persistence owner.</summary>
    private Bank80SystemState RequireSystem() =>
        _bindings.System ?? throw new InvalidOperationException("A resident door has no persistence owner.");

    /// <summary>Decodes a colored-door header into its color family and four-way orientation.</summary>
    internal static bool TryIdentifyColoredDoor(
        PlmHeaderId header,
        out ColoredDoorColor color,
        out ColoredDoorOrientation orientation)
    {
        PlmHeaderId firstHeader;
        if (header is >= PlmHeaderId.YellowDoorFacingLeft and
            <= PlmHeaderId.YellowDoorFacingDown)
        {
            color = ColoredDoorColor.Yellow;
            firstHeader = PlmHeaderId.YellowDoorFacingLeft;
        }
        else if (header is >= PlmHeaderId.GreenDoorFacingLeft and
            <= PlmHeaderId.GreenDoorFacingDown)
        {
            color = ColoredDoorColor.Green;
            firstHeader = PlmHeaderId.GreenDoorFacingLeft;
        }
        else if (header is >= PlmHeaderId.RedDoorFacingLeft and
            <= PlmHeaderId.RedDoorFacingDown)
        {
            color = ColoredDoorColor.Red;
            firstHeader = PlmHeaderId.RedDoorFacingLeft;
        }
        else
        {
            color = default;
            orientation = default;
            return false;
        }

        int byteOffset = (int)header - (int)firstHeader;
        if (byteOffset % 6 != 0 || byteOffset > 18)
        {
            orientation = default;
            return false;
        }
        orientation = ColoredDoorOrientations.AtTableIndex(byteOffset / 6);
        return true;
    }

    /// <summary>
    /// Spawns the blue-door entry selected by shootable BTS <c>$40..$43</c> at
    /// <c>$94:9F26-$9F2C</c> and applies setup <c>$84:C7BB</c>.
    /// </summary>
    /// <remarks>
    /// The cap's top/left origin begins as a type-$C shootable solid. Setup changes only
    /// that origin to type $8, preserving its twelve-bit tile index; the ROM instruction
    /// list then animates all four cap blocks and ends on the ordinary air-door frame.
    /// A power bomb is the one rejected projectile family. Exhausting all forty PLM slots
    /// silently loses the request, matching <c>Spawn_PLM_to_CurrentBlockIndex</c>.
    /// </remarks>
    public bool TrySpawnBlueDoorOpening(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior behavior,
        SamusProjectileTypeWord projectileType)
    {
        Ensure.NotNull(level);
        if (!behavior.TryGetBlueDoorOrientation(out ColoredDoorOrientation orientation))
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                behavior,
                "Blue-door shootable BTS must be $40 through $43.");
        }

        // Setup_BlueDoor masks the native projectile word with $0F00. A power-bomb
        // collision therefore deletes the just-allocated PLM without touching the cap.
        if (projectileType.Family == SamusProjectileFamily.PowerBomb)
            return false;

        PlmHeaderId headerPointer = orientation switch
        {
            ColoredDoorOrientation.Left => PlmHeaderId.BlueDoorFacingLeft,
            ColoredDoorOrientation.Right => PlmHeaderId.BlueDoorFacingRight,
            ColoredDoorOrientation.Up => PlmHeaderId.BlueDoorFacingUp,
            ColoredDoorOrientation.Down => PlmHeaderId.BlueDoorFacingDown,
            _ => throw new InvalidOperationException(
                "Validated blue-door BTS escaped its four-way header table."),
        };

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            slot.Clear();
            slot.Active = true;
            slot.HeaderPointer = headerPointer;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.StartAnimation(new BlueDoorPlmAnimation(BlueDoorPlmProgramDefinitions.Opening(orientation)));
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.PreInstruction = PlmPreInstructions.Inert;
            slot.RoomArgument = 0;
            slot.LoopTimer = 0;
            slot.Family = null;

            // `$84:C7D3-$C7DD` is a direct LevelData write, not a PLM draw. The first
            // animated draw occurs on this actor's next handler pass and owns the VRAM
            // update, while collision observes the type-$8 origin immediately.
            ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
            level.SetForegroundEntry(blockIndex, new RoomLevelWord(originalWord).WithCollisionType(RoomCollisionType.SolidBlock).Raw);
            return true;
        }

        return false;
    }
}
