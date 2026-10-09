using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored condition gates for bank-$84's grey door family.</summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>Progression-state owner used by resident grey doors to inspect events and persist successful openings.</summary>
    private Bank80SystemState? _greyDoorSystem;
    /// <summary>Area whose boss and miniboss flags satisfy area-scoped grey-door conditions.</summary>
    private AreaId _greyDoorArea;
    /// <summary>Live query for the Tourian statue sequence's completion condition.</summary>
    private Func<bool>? _isTourianStatueFinished;

    /// <summary>Advances one resident grey-door PLM, consuming locked hits or handing active instruction lists to the shared interpreter.</summary>
    /// <param name="bus">Address space used to read authored PLM instruction operands.</param>
    /// <param name="level">Room collision data modified when an already-open door becomes a blue door.</param>
    /// <param name="streamer">Background streamer used by the door's initial or conversion draw.</param>
    /// <param name="slot">Resident PLM slot and its condition-gated door state.</param>
    /// <param name="layer1XPosition">Current horizontal layer-one position for draw placement.</param>
    /// <param name="layer1YPosition">Current vertical layer-one position for draw placement.</param>
    /// <param name="bg1XOffset">BG1 horizontal offset used by the draw routine.</param>
    /// <param name="enemyDeaths">Current room enemy-death count.</param>
    /// <param name="enemyDeathQuota">Room-configured count required by quota-gated doors.</param>
    /// <returns>True when this pre-pass consumed the actor for the frame; false when the shared interpreter should process its list.</returns>
    private bool TryStepGreyDoor(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset,
        ushort enemyDeaths,
        byte enemyDeathQuota)
    {
        GreyDoorPlmState? door = slot.GreyDoor;
        if (door is null)
            return false;

        // The ordinary grey-door closing list ends immediately before its first list and
        // intentionally falls through to it; unlike Bomb Torizo's special list, there is
        // no explicit Goto to intercept. Restore the same resident family when the native
        // list cursor reaches that boundary. The final closing draw is already the fully
        // closed frame, so marking it complete also avoids drawing it twice.
        if (door.Phase == GreyDoorPhase.Closing &&
            slot.InstructionPointer == door.InitialList)
        {
            slot.PreInstruction = 0;
            door.Phase = GreyDoorPhase.Locked;
            door.InitialDrawCompleted = true;
            door.HasPendingHit = false;
        }

        // Resident room-entry closers execute the header's second list in the common
        // interpreter, then return to InitialList. Retaining this discriminator is what
        // lets that return restore the ordinary condition-gated door owner.
        if (door.Phase == GreyDoorPhase.Closing)
            return false;

        if (door.Phase == GreyDoorPhase.ConvertToBlue)
        {
            // Goto_if_room_argument_door_is_set selects the ordinary closed-blue list.
            // Its PLM_BTS_Y opcode and single draw are observable immediately on reload.
            byte blueBts = unchecked((byte)(
                RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + (byte)door.Orientation));
            ushort drawPointer = ReadProgramWord(
                bus,
                unchecked((ushort)(door.ClosedBlueList + 5)));
            level.SetBehavior(slot.BlockIndex, blueBts);
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
            slot.GreyDoor = null;
            return true;
        }

        if (!door.InitialDrawCompleted)
        {
            DrawPlmInstruction(
                bus,
                level,
                streamer,
                slot.BlockIndex,
                door.ClosedGreyDraw,
                layer1XPosition,
                layer1YPosition,
                bg1XOffset);
            door.InitialDrawCompleted = true;
        }

        if (door.Phase == GreyDoorPhase.Locked)
        {
            if (!IsGreyDoorConditionSatisfied(door.Condition, enemyDeaths, enemyDeathQuota))
            {
                // Play_Dud_Sound_if_Shot consumes the shared PLM timer whether or not it
                // was nonzero. This is why a rejected shot never survives until unlock.
                if (door.HasPendingHit)
                    _soundRequests.Add(CreateSoundRequest(SoundEffectLibrary2Sounds.DoorOpening, MaximumQueued: 6));
                door.HasPendingHit = false;
                door.PendingProjectileType = default;
                slot.LoopTimer = 0;
                return true;
            }

            if (door.Condition == GreyDoorCondition.EnemyDeathQuota)
            {
                (_greyDoorSystem ?? throw new InvalidOperationException(
                    "A resident grey door has no progression-state owner."))
                    .SetEvent(EventNumber.ZebesAwake);
            }

            // Goto_Link_Instruction clears shot status, sets timer one, and selects the
            // activation link. That link installs the later shot link/pre-instruction and
            // falls straight into the repeating flash list on this same PLM handler pass.
            // Clearing the hit here is essential: the quota-completing frame cannot also
            // open the door even if a projectile happened to collide during that frame.
            door.HasPendingHit = false;
            door.PendingProjectileType = default;
            door.Phase = GreyDoorPhase.Flashing;
            slot.InstructionPointer = door.FlashList;
            slot.InstructionTimer = 1;
            slot.LoopTimer = 0;
            return false;
        }

        if (door.Phase == GreyDoorPhase.Flashing && door.HasPendingHit)
        {
            door.HasPendingHit = false;
            door.PendingProjectileType = default;
            if (unchecked((short)slot.RoomArgument) >= 0)
            {
                (_greyDoorSystem ?? throw new InvalidOperationException(
                    "A resident grey door has no persistence-state owner."))
                    .SetOpenedDoorBit(slot.RoomArgument);
                slot.RoomArgument |= 0x8000;
            }

            // `$84:8A91` uses threshold one for every grey door. Its only successful
            // branch target is the ROM pointer extracted above, so the shared interpreter
            // remains responsible for sound seven, all four authored draws, and deletion.
            door.Phase = GreyDoorPhase.Opening;
            slot.InstructionPointer = door.OpeningList;
            slot.InstructionTimer = 1;
        }

        // Flashing and opening streams contain only timer/draw pairs, Goto, sound, and
        // Delete—all shared opcodes. Returning false deliberately hands those streams to
        // the ordinary cartridge instruction interpreter below this semantic pre-pass.
        return false;
    }

    /// <summary>Evaluates the selected progression, room-quota, statue, or critter condition for a resident grey door.</summary>
    /// <param name="condition">Native condition-table entry attached during room setup.</param>
    /// <param name="enemyDeaths">Current room enemy-death count for quota conditions.</param>
    /// <param name="enemyDeathQuota">Required death count for quota conditions.</param>
    /// <returns>True only when the configured condition currently allows the door to unlock.</returns>
    private bool IsGreyDoorConditionSatisfied(
        GreyDoorCondition condition,
        ushort enemyDeaths,
        byte enemyDeathQuota)
    {
        Bank80SystemState system = _greyDoorSystem ?? throw new InvalidOperationException(
            "A resident grey door has no progression-state owner.");
        return condition switch
        {
            GreyDoorCondition.AreaBossDefeated =>
                system.HasAnyBossBits(_greyDoorArea, BossBits.AreaBoss),
            GreyDoorCondition.AreaMiniBossDefeated =>
                system.HasAnyBossBits(_greyDoorArea, BossBits.AreaMiniBoss),
            GreyDoorCondition.AreaTorizoDefeated =>
                system.HasAnyBossBits(_greyDoorArea, BossBits.AreaTorizo),
            GreyDoorCondition.EnemyDeathQuota => enemyDeaths >= enemyDeathQuota,
            GreyDoorCondition.Never => false,
            GreyDoorCondition.TourianStatueFinished =>
                _isTourianStatueFinished?.Invoke() == true,
            GreyDoorCondition.CrittersEscaped =>
                system.HasEvent(EventNumber.CrittersEscaped),
            _ => throw new InvalidDataException(
                $"Unknown grey-door condition {(byte)condition}.")
        };
    }

    /// <summary>Applies the shared grey-door/colored-door collision setup while preserving the block's visual payload.</summary>
    /// <param name="level">Room level whose collision and behavior planes are updated.</param>
    /// <param name="blockIndex">Block changed to a resident-PLM projectile trigger.</param>
    private static void ApplyGreyDoorSetup(RoomLevelData level, int blockIndex)
    {
        // Setup_GreyDoor and Setup_ColoredDoor share the exact `$C044` collision write.
        // Preserve the authored visual payload while replacing only type and BTS.
        ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetForegroundEntry(blockIndex, unchecked((ushort)((originalWord & 0x0fff) | 0xc000)));
        level.SetBehavior(blockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
    }

    /// <summary>Runs Setup_GreyDoor on a slot allocated in native record order.</summary>
    private static void SetupGreyDoorSlot(
        ISnesAddressSpace bus,
        RoomLevelData level,
        Bank80SystemState system,
        PlmSlot slot,
        ColoredDoorOrientation orientation)
    {
        ushort rawRoomArgument = slot.RoomArgument;
        int conditionOffset = ((rawRoomArgument >> 8) & 0x7c) >> 1;
        if ((conditionOffset & 1) != 0 || conditionOffset > 12)
        {
            throw new InvalidDataException(
                $"Grey-door header ${slot.HeaderPointer:X4} selected invalid condition " +
                $"offset ${conditionOffset:X2} from room argument ${rawRoomArgument:X4}.");
        }

        GreyDoorCondition condition = (GreyDoorCondition)(conditionOffset >> 1);
        slot.RoomArgument = unchecked((ushort)(rawRoomArgument & 0x83ff));
        ApplyGreyDoorSetup(level, slot.BlockIndex);

        ushort initialList = slot.InstructionPointer;
        ushort closedBlueList = ReadProgramWord(bus, unchecked((ushort)(initialList + 2)));
        ushort activationList = ReadProgramWord(bus, unchecked((ushort)(initialList + 6)));
        ushort closedGreyDraw = ReadProgramWord(bus, unchecked((ushort)(initialList + 12)));
        ushort openTriggerList = ReadProgramWord(bus, unchecked((ushort)(activationList + 2)));
        ushort flashList = unchecked((ushort)(activationList + 8));
        ushort openingList = ReadProgramWord(bus, unchecked((ushort)(openTriggerList + 3)));
        bool wasOpened = unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasOpenedDoorBit(slot.RoomArgument);

        slot.GreyDoor = new GreyDoorPlmState(
            orientation,
            condition,
            initialList,
            closedBlueList,
            closedGreyDraw,
            flashList,
            openingList,
            wasOpened ? GreyDoorPhase.ConvertToBlue : GreyDoorPhase.Locked);
    }

    /// <summary>Recognizes a native grey-door header and decodes its orientation, including the Bomb Torizo alias.</summary>
    /// <param name="header">PLM header pointer read from the room's authored PLM list.</param>
    /// <param name="orientation">Receives the matched facing when the header is a supported grey door.</param>
    /// <returns>True when the header identifies a grey-door orientation.</returns>
    private static bool TryIdentifyGreyDoor(
        ushort header,
        out ColoredDoorOrientation orientation)
    {
        if (header == RoomPlmHeaders.BombTorizoGreyDoor)
        {
            orientation = ColoredDoorOrientation.Right;
            return true;
        }

        if (header is < RoomPlmHeaders.GreyDoorFacingLeft or
            > RoomPlmHeaders.GreyDoorFacingDown)
        {
            orientation = default;
            return false;
        }

        int byteOffset = header - RoomPlmHeaders.GreyDoorFacingLeft;
        if (byteOffset % 6 != 0 || byteOffset > 18)
        {
            orientation = default;
            return false;
        }
        orientation = (ColoredDoorOrientation)(byteOffset / 6);
        return true;
    }
}

/// <summary>The seven entries in <c>$84:BE4B</c>'s grey-door condition table.</summary>
public enum GreyDoorCondition : byte
{
    /// <summary>Condition-table index 0: the current area's major-boss bit must be set.</summary>
    AreaBossDefeated,
    /// <summary>Condition-table index 1: the current area's miniboss bit must be set.</summary>
    AreaMiniBossDefeated,
    /// <summary>Condition-table index 2: the current area's Torizo boss bit must be set.</summary>
    AreaTorizoDefeated,
    /// <summary>Condition-table index 3: room enemy deaths must reach the configured quota; satisfying it also sets the Zebes-awake event.</summary>
    EnemyDeathQuota,
    /// <summary>Condition-table index 4: this door remains locked regardless of shots or progression.</summary>
    Never,
    /// <summary>Condition-table index 5: the live Tourian statue owner must report its sequence finished.</summary>
    TourianStatueFinished,
    /// <summary>Condition-table index 6: the persistent critters-escaped event must be set.</summary>
    CrittersEscaped,
}

/// <summary>Debugger-visible phase of one resident grey-door PLM.</summary>
public enum GreyDoorPhase : byte
{
    /// <summary>The closed grey cap polls its condition and consumes rejected hits; the hit present on the unlock pass is also cleared.</summary>
    Locked,
    /// <summary>The condition has unlocked the door and its flash list repeats until a later projectile trigger starts opening.</summary>
    Flashing,
    /// <summary>A post-unlock hit has recorded the persistent door bit and handed opening sound, timed draws, and deletion to the shared interpreter.</summary>
    Opening,
    /// <summary>Room setup found the door already opened and will install the closed blue cap and BTS, then release the resident slot.</summary>
    ConvertToBlue,
    /// <summary>The room-entry closing list runs in the shared interpreter before returning this resident actor to its locked initial list.</summary>
    Closing,
}

/// <summary>Resident per-door state retaining native list pointers and the condition/opening lifecycle.</summary>
/// <param name="orientation">Facing that determines the closed blue-door behavior value.</param>
/// <param name="condition">Progression condition polled while the door is locked.</param>
/// <param name="initialList">Native closed-grey instruction list and return target after room-entry closing.</param>
/// <param name="closedBlueList">Native list used to draw and convert an already-open door to blue.</param>
/// <param name="closedGreyDraw">Native draw instruction for the initial closed-grey appearance.</param>
/// <param name="flashList">Native repeating list entered when the lock condition becomes satisfied.</param>
/// <param name="openingList">Native opening sequence entered after a successful post-unlock hit.</param>
/// <param name="phase">Initial lifecycle state selected during room setup.</param>
internal sealed class GreyDoorPlmState(
    ColoredDoorOrientation orientation,
    GreyDoorCondition condition,
    ushort initialList,
    ushort closedBlueList,
    ushort closedGreyDraw,
    ushort flashList,
    ushort openingList,
    GreyDoorPhase phase)
{
    /// <summary>Physical facing of the door used when selecting its blue-door BTS value.</summary>
    public ColoredDoorOrientation Orientation { get; } = orientation;
    /// <summary>Progression or room condition checked before the door begins flashing.</summary>
    public GreyDoorCondition Condition { get; } = condition;
    /// <summary>Closed-grey instruction list and target restored after the resident closing list completes.</summary>
    public ushort InitialList { get; } = initialList;
    /// <summary>Instruction list that applies the closed blue cap for a door already opened in an earlier visit.</summary>
    public ushort ClosedBlueList { get; } = closedBlueList;
    /// <summary>Instruction pointer for drawing the closed grey cap when the resident actor first runs.</summary>
    public ushort ClosedGreyDraw { get; } = closedGreyDraw;
    /// <summary>Instruction list that repeats the unlocked flashing animation until a projectile hit.</summary>
    public ushort FlashList { get; } = flashList;
    /// <summary>Instruction list for the shared-interpreter opening sound, draws, and deletion sequence.</summary>
    public ushort OpeningList { get; } = openingList;
    /// <summary>Current locked, flashing, opening, conversion, or room-entry closing lifecycle phase.</summary>
    public GreyDoorPhase Phase { get; set; } = phase;
    /// <summary>Prevents the initial closed-grey draw from being applied more than once.</summary>
    public bool InitialDrawCompleted { get; set; }
    /// <summary>Whether a projectile hit is pending for the current condition or flashing check.</summary>
    public bool HasPendingHit { get; set; }
    /// <summary>Projectile category retained with a pending hit until the native hit path consumes it.</summary>
    public SamusProjectileTypeWord PendingProjectileType { get; set; }
}
