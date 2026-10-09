using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored door-cap reactions owned by bank $84's PLM pool.</summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>Persistence state used to record opened bits when resident colored doors reach their hit threshold.</summary>
    private Bank80SystemState? _coloredDoorSystem;

    /// <summary>Applies the shootable-solid collision and resident-projectile-trigger BTS for a colored cap.</summary>
    /// <param name="level">Room level whose PLM block metadata is updated.</param>
    /// <param name="blockIndex">Index of the cap's collision block.</param>
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
    private static void SetupColoredDoorSlot(
        ISnesAddressSpace bus,
        RoomLevelData level,
        Bank80SystemState system,
        PlmSlot slot,
        ColoredDoorColor color,
        ColoredDoorOrientation orientation)
    {
        ApplyColoredDoorSetup(level, slot.BlockIndex);

        ushort initialList = slot.InstructionPointer;
        ushort closedBlueList = ReadProgramWord(bus, unchecked((ushort)(initialList + 2)));
        ushort hitList = ReadProgramWord(bus, unchecked((ushort)(initialList + 6)));
        byte hitThreshold = ReadProgramByte(bus, unchecked((ushort)(hitList + 2)));
        ushort openingList = ReadProgramWord(bus, unchecked((ushort)(hitList + 3)));
        ushort coloredClosedDraw = ReadProgramWord(bus, unchecked((ushort)(initialList + 14)));
        bool wasOpened = unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasOpenedDoorBit(slot.RoomArgument);

        slot.ColoredDoor = new ColoredDoorPlmState(
            color,
            orientation,
            initialList,
            closedBlueList,
            hitList,
            openingList,
            coloredClosedDraw,
            hitThreshold,
            wasOpened ? ColoredDoorPhase.ConvertToBlue : ColoredDoorPhase.Waiting);
    }

    /// <summary>Advances a resident colored-door actor through its waiting, hit, flash, opening, or room-entry phase.</summary>
    /// <param name="bus">Address space used to read native list operands and draw data.</param>
    /// <param name="level">Room level receiving collision or colored-cap updates.</param>
    /// <param name="streamer">Background streamer receiving draw-list changes.</param>
    /// <param name="slot">PLM actor whose attached colored-door state is advanced.</param>
    /// <param name="layer1XPosition">Current horizontal Layer 1 position.</param>
    /// <param name="layer1YPosition">Current vertical Layer 1 position.</param>
    /// <param name="bg1XOffset">Horizontal BG1 offset used to place the PLM draw.</param>
    /// <returns><see langword="true"/> when this actor remains owned by the colored-door handler for the pass.</returns>
    private bool TryStepColoredDoor(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
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
            ushort drawPointer = ReadProgramWord(
                bus,
                unchecked((ushort)(door.ClosedBlueList + 5)));
            level.SetPlmBehavior(slot.BlockIndex, blueBts);
            DrawPlmInstruction(
                bus,
                level,
                streamer,
                slot.BlockIndex,
                drawPointer,
                layer1XPosition,
                layer1YPosition,
                bg1XOffset);
            slot.Active = false;
            slot.ColoredDoor = null;
            return true;
        }

        if (!door.InitialDrawCompleted)
        {
            DrawPlmInstruction(
                bus,
                level,
                streamer,
                slot.BlockIndex,
                door.ColoredClosedDraw,
                layer1XPosition,
                layer1YPosition,
                bg1XOffset);
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
            _soundRequests.Add(CreateSoundRequest(SoundEffectLibrary2Sounds.DoorOpening, MaximumQueued: 6));
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
                (_coloredDoorSystem ?? throw new InvalidOperationException(
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

    /// <summary>Decodes a native colored-door header into its color family and six-byte orientation slot.</summary>
    /// <param name="header">Bank-$84 PLM header pointer to classify.</param>
    /// <param name="color">Receives the matching yellow, green, or red door family.</param>
    /// <param name="orientation">Receives the left, right, up, or down cap orientation.</param>
    /// <returns><see langword="true"/> only for one of the recognized colored-door headers.</returns>
    private static bool TryIdentifyColoredDoor(
        ushort header,
        out ColoredDoorColor color,
        out ColoredDoorOrientation orientation)
    {
        ushort firstHeader;
        if (header is >= RoomPlmHeaders.YellowDoorFacingLeft and
            <= RoomPlmHeaders.YellowDoorFacingDown)
        {
            color = ColoredDoorColor.Yellow;
            firstHeader = RoomPlmHeaders.YellowDoorFacingLeft;
        }
        else if (header is >= RoomPlmHeaders.GreenDoorFacingLeft and
            <= RoomPlmHeaders.GreenDoorFacingDown)
        {
            color = ColoredDoorColor.Green;
            firstHeader = RoomPlmHeaders.GreenDoorFacingLeft;
        }
        else if (header is >= RoomPlmHeaders.RedDoorFacingLeft and
            <= RoomPlmHeaders.RedDoorFacingDown)
        {
            color = ColoredDoorColor.Red;
            firstHeader = RoomPlmHeaders.RedDoorFacingLeft;
        }
        else
        {
            color = default;
            orientation = default;
            return false;
        }

        int byteOffset = header - firstHeader;
        if (byteOffset % 6 != 0 || byteOffset > 18)
        {
            orientation = default;
            return false;
        }
        orientation = (ColoredDoorOrientation)(byteOffset / 6);
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
        ArgumentNullException.ThrowIfNull(level);
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
        ushort headerPointer = orientation switch
        {
            ColoredDoorOrientation.Left => RoomPlmHeaders.BlueDoorFacingLeft,
            ColoredDoorOrientation.Right => RoomPlmHeaders.BlueDoorFacingRight,
            ColoredDoorOrientation.Up => RoomPlmHeaders.BlueDoorFacingUp,
            ColoredDoorOrientation.Down => RoomPlmHeaders.BlueDoorFacingDown,
            _ => throw new InvalidOperationException(
                "Validated blue-door BTS escaped its four-way header table."),
        };

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
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

/// <summary>The three projectile-gated door families in header order.</summary>
public enum ColoredDoorColor : byte
{
    /// <summary>Yellow cap, which accepts Power Bomb hits.</summary>
    Yellow,
    /// <summary>Green cap, which accepts Super Missile hits.</summary>
    Green,
    /// <summary>Red cap, which accepts Missile or Super Missile hits; a Super Missile immediately satisfies its hit threshold.</summary>
    Red,
}

/// <summary>Door-cap orientation shared by colored and blue BTS tables.</summary>
public enum ColoredDoorOrientation : byte
{
    /// <summary>Left-facing cap; orientation index 0 corresponds to blue-door BTS $40.</summary>
    Left,
    /// <summary>Right-facing cap; orientation index 1 corresponds to blue-door BTS $41.</summary>
    Right,
    /// <summary>Upward-facing cap; orientation index 2 corresponds to blue-door BTS $42.</summary>
    Up,
    /// <summary>Downward-facing cap; orientation index 3 corresponds to blue-door BTS $43.</summary>
    Down,
}

/// <summary>Debugger-visible phase of one resident colored-door actor.</summary>
public enum ColoredDoorPhase : byte
{
    /// <summary>Resident cap has drawn its closed color and waits for projectile triggers while its instruction list sleeps.</summary>
    Waiting,
    /// <summary>A nonfatal accepted hit runs the shared blue/colored flash list before returning the resident actor to its sleeping wait.</summary>
    Flashing,
    /// <summary>The accepted-hit threshold has been reached; the shared interpreter runs the opening list after recording the door's persistent opened bit.</summary>
    Opening,
    /// <summary>Room setup found the persistent opened bit and will replace the colored cap with the closed blue-door draw and BTS, then release the slot.</summary>
    ConvertToBlue,
    /// <summary>Room-entry closing draws run through the shared interpreter before handing the same resident slot back to its initial waiting list.</summary>
    Closing,
}
