using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Coloured and blue door PLMs: their setup, shot reactions, opening and the door-closing spawn after a transition.</summary>
internal sealed class DoorPlms
{
    /// <summary>Stores bindings.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Stores drawing.</summary>
    private readonly PlmBlockDrawing _drawing;
    /// <summary>Stores slot table.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Stores sounds.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Initializes a new door plms instance.</summary>
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
        ISnesAddressSpace bus,
        RoomLevelData level,
        CartridgeDoorHeader door,
        Bank80SystemState system)
    {
        Ensure.NotNull(bus);
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

            // Header+4 is definition metadata, not active memory. All resident
            // closing actors, including the escape gate, resolve it by identity.
            ushort closingList = ResidentDoorClosingDefinitions.Resolve(resident.HeaderPointer);
            if (closingList < 0x8000)
            {
                throw new InvalidDataException(
                    $"Door-cap PLM $84:{(int)resident.HeaderPointer:X4} at block {blockIndex} " +
                    $"has invalid second instruction list $84:{closingList:X4}.");
            }

            resident.InstructionPointer = closingList;
            resident.InstructionTimer = 1;

            // Native retains every scalar owned by the resident slot while only replacing
            // its instruction pointer. Preserve the equivalent C# family data and mark the
            // interval explicitly: the common interpreter owns the secondary list, while
            // its final Goto returns this same actor to the original door-family program.
            if (resident.ColoredDoor is not null)
                resident.ColoredDoor.Phase = ColoredDoorPhase.Closing;
            if (resident.GreyDoor is not null)
                resident.GreyDoor.Phase = GreyDoorPhase.Closing;
            return true;
        }

        if (DoorClosingPlmRomData.GetDefinition(door.Orientation) is not { } fallback)
            return false;

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = fallback.Header;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = fallback.InitialInstructionList;
            slot.InstructionTimer = 1;
            SetupDoorTransitionDeactivatedSlot(level, slot);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Completes a resident door's secondary-list close when its final Goto returns to the
    /// header's first list. This is the semantic equivalent of the native slot retaining
    /// its room argument, variable, link register, and family pre-instruction state.
    /// </summary>
    internal bool TryCompleteResidentDoorClosing(ISnesAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, ushort target, PlmView view)
    {
        if (slot.ColoredDoor is { Phase: ColoredDoorPhase.Closing } coloredDoor &&
            target == coloredDoor.InitialList)
        {
            slot.InstructionPointer = target;
            slot.InstructionTimer = 1;
            slot.PreInstruction = 0;
            coloredDoor.Phase = ColoredDoorPhase.Waiting;
            coloredDoor.InitialDrawCompleted = true;
            coloredDoor.HasPendingHit = false;
            _drawing.DrawPlmInstruction(bus, level, streamer, slot.BlockIndex, coloredDoor.ColoredClosedDraw, view);
            return true;
        }

        if (slot.GreyDoor is { Phase: GreyDoorPhase.Closing } greyDoor &&
            target == greyDoor.InitialList)
        {
            slot.InstructionPointer = target;
            slot.InstructionTimer = 1;
            slot.PreInstruction = 0;
            greyDoor.Phase = GreyDoorPhase.Locked;
            greyDoor.InitialDrawCompleted = true;
            greyDoor.HasPendingHit = false;
            _drawing.DrawPlmInstruction(bus, level, streamer, slot.BlockIndex, greyDoor.ClosedGreyDraw, view);
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

    /// <summary>Applies colored door setup.</summary>
    private static void ApplyColoredDoorSetup(RoomLevelData level, int blockIndex)
    {
        // Setup `$84:C7B1` preserves the visual twelve bits, installs shootable-solid
        // collision, and selects BTS $44 so subsequent projectiles find this resident PLM.
        ushort originalWord = level.GetPlmCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetPlmForegroundEntry(
            blockIndex,
            unchecked((ushort)((originalWord & 0x0fff) | 0xc000)));
        level.SetPlmBehavior(blockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
    }

    /// <summary>
    /// Executes the colored-door header setup against the slot already allocated by the
    /// sequential room-population loader. Allocation deliberately does not live here:
    /// native <c>Spawn_Room_PLM</c> chooses the physical ID before calling this routine.
    /// </summary>
    internal static void SetupColoredDoorSlot(
        ISnesAddressSpace bus,
        RoomLevelData level,
        Bank80SystemState system,
        PlmSlot slot,
        ColoredDoorColor color,
        ColoredDoorOrientation orientation)
    {
        ApplyColoredDoorSetup(level, slot.BlockIndex);

        ushort initialList = slot.InstructionPointer;
        ushort closedBlueList = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(initialList + 2)));
        ushort hitList = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(initialList + 6)));
        byte hitThreshold = RoomPlmSystem.ReadProgramByte(bus, unchecked((ushort)(hitList + 2)));
        ushort openingList = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(hitList + 3)));
        ushort coloredClosedDraw = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(initialList + 14)));
        bool wasOpened = unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasOpenedDoorBit(slot.RoomArgument);

        slot.ColoredDoor = new ColoredDoorPlmState(
            color,
            orientation,
            new ColoredDoorPrograms(initialList, closedBlueList, hitList, openingList, coloredClosedDraw),
            hitThreshold,
            wasOpened ? ColoredDoorPhase.ConvertToBlue : ColoredDoorPhase.Waiting);
    }

    /// <summary>Attempts to step colored door.</summary>
    internal bool TryStepColoredDoor(ISnesAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, PlmView view)
    {
        ColoredDoorPlmState? door = slot.ColoredDoor;
        if (door is null)
            return false;

        // The four ordinary coloured-door closing lists are laid out immediately before
        // their first lists and deliberately fall through instead of ending in Goto.
        // Native therefore reaches the same resident owner's first instruction on the
        // handler pass after the final closing draw. Re-enter the semantic owner at that
        // exact cursor rather than sending its door-bit opcode through an unrelated PLM
        // family dispatcher.
        if (door.Phase == ColoredDoorPhase.Closing &&
            slot.InstructionPointer == door.InitialList)
        {
            slot.PreInstruction = 0;
            door.Phase = ColoredDoorPhase.Waiting;
            door.InitialDrawCompleted = true;
            door.HasPendingHit = false;
        }

        // A room-entry close temporarily runs the header's second cartridge list through
        // the shared interpreter. The semantic owner remains attached so its final Goto
        // can hand the same physical slot back to this family without reconstructing it.
        if (door.Phase == ColoredDoorPhase.Closing)
            return false;

        if (door.Phase == ColoredDoorPhase.ConvertToBlue)
        {
            // The initial Goto_if_room_argument_door_is_set branch selects a short list:
            // PLM_BTS_Y, one timed closed-blue draw, then delete. Execute its observable
            // setup/draw atomically here while retaining the cartridge pointers and art.
            byte blueBts = unchecked((byte)(
                RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + (byte)door.Orientation));
            ushort drawPointer = RoomPlmSystem.ReadProgramWord(
                bus,
                unchecked((ushort)(door.ClosedBlueList + 5)));
            level.SetPlmBehavior(slot.BlockIndex, blueBts);
            _drawing.DrawPlmInstruction(bus, level, streamer, slot.BlockIndex, drawPointer, view);
            slot.Active = false;
            slot.ColoredDoor = null;
            return true;
        }

        if (!door.InitialDrawCompleted)
        {
            _drawing.DrawPlmInstruction(bus, level, streamer, slot.BlockIndex, door.ColoredClosedDraw, view);
            door.InitialDrawCompleted = true;
        }

        if (!door.HasPendingHit)
            return door.Phase == ColoredDoorPhase.Waiting;

        SamusProjectileFamily projectileFamily = door.PendingProjectileType.Family;
        door.HasPendingHit = false;
        bool accepted = door.Color switch
        {
            ColoredDoorColor.Yellow => projectileFamily == SamusProjectileFamily.PowerBomb,
            ColoredDoorColor.Green => projectileFamily == SamusProjectileFamily.SuperMissile,
            ColoredDoorColor.Red => projectileFamily is
                SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile,
            _ => throw new InvalidDataException($"Unknown colored-door family {door.Color}."),
        };
        if (!accepted)
        {
            // Each native pre-instruction queues library-two sound $57 for a rejected
            // projectile. A flash already in progress is not interrupted by that dud.
            _sounds._soundRequests.Add(_sounds.CreateSoundRequest(SoundEffectLibrary2Sounds.DoorOpening, MaximumQueued: 6));
            return door.Phase == ColoredDoorPhase.Waiting;
        }

        // Red-door setup writes $77 before the shared INC instruction when struck by a
        // Super Missile. Unsigned wrap therefore guarantees the very next increment meets
        // the five-hit threshold, exactly reproducing the cartridge's one-super behavior.
        if (door.Color == ColoredDoorColor.Red &&
            projectileFamily == SamusProjectileFamily.SuperMissile)
            door.HitCounter = 0x77;
        door.HitCounter = unchecked((byte)(door.HitCounter + 1));

        if (door.HitCounter >= door.HitThreshold)
        {
            if (unchecked((short)slot.RoomArgument) >= 0)
            {
                (_bindings.System ?? throw new InvalidOperationException(
                    "A resident colored door has no persistence owner."))
                    .SetOpenedDoorBit(slot.RoomArgument);
                slot.RoomArgument |= 0x8000;
            }

            door.Phase = ColoredDoorPhase.Opening;
            slot.PreInstruction = 0;
            slot.InstructionPointer = door.OpeningList;
            slot.InstructionTimer = 1;
            return false;
        }

        // The five bytes after INC/threshold/open-target are the first operation in the
        // cartridge's nonfatal blue/colored flash list. Later Goto returns to Sleep, where
        // the common interpreter hands the actor back to Waiting.
        door.Phase = ColoredDoorPhase.Flashing;
        slot.InstructionPointer = unchecked((ushort)(door.HitList + 5));
        slot.InstructionTimer = 1;
        return false;
    }

    /// <summary>Attempts to identify colored door.</summary>
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

        ushort instructionPointer = orientation switch
        {
            ColoredDoorOrientation.Left => RoomPlmInstructionLists.BlueDoorFacingLeftOpening,
            ColoredDoorOrientation.Right => RoomPlmInstructionLists.BlueDoorFacingRightOpening,
            ColoredDoorOrientation.Up => RoomPlmInstructionLists.BlueDoorFacingUpOpening,
            ColoredDoorOrientation.Down => RoomPlmInstructionLists.BlueDoorFacingDownOpening,
            _ => throw new InvalidOperationException(
                "Validated blue-door BTS escaped its four-way instruction table."),
        };
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

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = headerPointer;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = instructionPointer;
            slot.InstructionTimer = 1;
            slot.PreInstruction = 0;
            slot.RoomArgument = 0;
            slot.LoopTimer = 0;
            slot.Item = null;

            // `$84:C7D3-$C7DD` is a direct LevelData write, not a PLM draw. The first
            // animated draw occurs on this actor's next handler pass and owns the VRAM
            // update, while collision observes the type-$8 origin immediately.
            ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
            level.SetForegroundEntry(blockIndex, (ushort)((originalWord & 0x0fff) | 0x8000));
            return true;
        }

        return false;
    }
}
