using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Grey door PLMs: condition identification (boss, mini-boss, Torizo, statue, event), flash and opening.</summary>
internal sealed class GreyDoorPlms
{
    /// <summary>Room progression and statue state used to evaluate resident-door unlock conditions.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Draws closed-grey and converted-blue tiles into the room level and background streamer.</summary>
    private readonly PlmBlockDrawing _drawing;
    /// <summary>Queues the cartridge's dud sound for shots rejected while the door is locked.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Binds progression checks, block drawing, and PLM sound requests.</summary>
    internal GreyDoorPlms(PlmRoomBindings bindings, PlmBlockDrawing drawing, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _drawing = drawing;
        _sounds = sounds;
    }

    /// <summary>Handles condition gating, closed-blue conversion, and hit-triggered opening before shared instruction interpretation.</summary>
    internal bool TryStepGreyDoor(ISnesAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, PlmView view, ushort enemyDeaths, byte enemyDeathQuota)
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
            ushort drawPointer = RoomPlmSystem.ReadProgramWord(
                bus,
                unchecked((ushort)(door.ClosedBlueList + 5)));
            level.SetBehavior(slot.BlockIndex, blueBts);
            _drawing.DrawPlmInstruction(bus, level, streamer, slot.BlockIndex, drawPointer, view);
            slot.Active = false;
            slot.GreyDoor = null;
            return true;
        }

        if (!door.InitialDrawCompleted)
        {
            _drawing.DrawPlmInstruction(bus, level, streamer, slot.BlockIndex, door.ClosedGreyDraw, view);
            door.InitialDrawCompleted = true;
        }

        if (door.Phase == GreyDoorPhase.Locked)
        {
            if (!IsGreyDoorConditionSatisfied(door.Condition, enemyDeaths, enemyDeathQuota))
            {
                // Play_Dud_Sound_if_Shot consumes the shared PLM timer whether or not it
                // was nonzero. This is why a rejected shot never survives until unlock.
                if (door.HasPendingHit)
                    _sounds._soundRequests.Add(_sounds.CreateSoundRequest(SoundEffectLibrary2Sounds.DoorOpening, MaximumQueued: 6));
                door.HasPendingHit = false;
                door.PendingProjectileType = default;
                slot.LoopTimer = 0;
                return true;
            }

            if (door.Condition == GreyDoorCondition.EnemyDeathQuota)
            {
                (_bindings.System ?? throw new InvalidOperationException(
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
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.LoopTimer = 0;
            return false;
        }

        if (door.Phase == GreyDoorPhase.Flashing && door.HasPendingHit)
        {
            door.HasPendingHit = false;
            door.PendingProjectileType = default;
            if (unchecked((short)slot.RoomArgument) >= 0)
            {
                (_bindings.System ?? throw new InvalidOperationException(
                    "A resident grey door has no persistence-state owner."))
                    .SetOpenedDoorBit(slot.RoomArgument);
                slot.RoomArgument |= 0x8000;
            }

            // `$84:8A91` uses threshold one for every grey door. Its only successful
            // branch target is the ROM pointer extracted above, so the shared interpreter
            // remains responsible for sound seven, all four authored draws, and deletion.
            door.Phase = GreyDoorPhase.Opening;
            slot.InstructionPointer = door.OpeningList;
            slot.InstructionTimer = NativeCountdown.Start(1);
        }

        // Flashing and opening streams contain only timer/draw pairs, Goto, sound, and
        // Delete—all shared opcodes. Returning false deliberately hands those streams to
        // the ordinary cartridge instruction interpreter below this semantic pre-pass.
        return false;
    }

    /// <summary>Evaluates boss, quota, statue, and event gates using live room progression state.</summary>
    private bool IsGreyDoorConditionSatisfied(
        GreyDoorCondition condition,
        ushort enemyDeaths,
        byte enemyDeathQuota)
    {
        Bank80SystemState system = _bindings.System ?? throw new InvalidOperationException(
            "A resident grey door has no progression-state owner.");
        return condition switch
        {
            GreyDoorCondition.AreaBossDefeated =>
                system.HasAnyBossBits(_bindings.Area, BossBits.AreaBoss),
            GreyDoorCondition.AreaMiniBossDefeated =>
                system.HasAnyBossBits(_bindings.Area, BossBits.AreaMiniBoss),
            GreyDoorCondition.AreaTorizoDefeated =>
                system.HasAnyBossBits(_bindings.Area, BossBits.AreaTorizo),
            GreyDoorCondition.EnemyDeathQuota => enemyDeaths >= enemyDeathQuota,
            GreyDoorCondition.Never => false,
            GreyDoorCondition.TourianStatueFinished =>
                _bindings.TourianStatue?.IsFinished == true,
            GreyDoorCondition.CrittersEscaped =>
                system.HasEvent(EventNumber.CrittersEscaped),
            _ => throw new InvalidDataException(
                $"Unknown grey-door condition {(byte)condition}.")
        };
    }

    /// <summary>Installs the resident projectile-trigger collision type while preserving the block's visual bits.</summary>
    private static void ApplyGreyDoorSetup(RoomLevelData level, int blockIndex)
    {
        // Setup_GreyDoor and Setup_ColoredDoor share the exact `$C044` collision write.
        // Preserve the authored visual payload while replacing only type and BTS.
        ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetForegroundEntry(blockIndex, unchecked((ushort)((originalWord & 0x0fff) | 0xc000)));
        level.SetBehavior(blockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
    }

    /// <summary>Runs Setup_GreyDoor on a slot allocated in native record order.</summary>
    internal static void SetupGreyDoorSlot(
        ISnesAddressSpace bus,
        RoomLevelData level,
        Bank80SystemState system,
        PlmSlot slot,
        ColoredDoorOrientation orientation)
    {
        ushort rawRoomArgument = slot.RoomArgument;
        int conditionOffset = ((SnesLittleEndian.HighByte(rawRoomArgument)) & 0x7c) >> 1;
        if ((conditionOffset & 1) != 0 || conditionOffset > 12)
        {
            throw new InvalidDataException(
                $"Grey-door header ${(int)slot.HeaderPointer:X4} selected invalid condition " +
                $"offset ${conditionOffset:X2} from room argument ${rawRoomArgument:X4}.");
        }

        GreyDoorCondition condition = GreyDoorConditions.AtTableIndex(conditionOffset >> 1);
        slot.RoomArgument = unchecked((ushort)(rawRoomArgument & 0x83ff));
        ApplyGreyDoorSetup(level, slot.BlockIndex);

        ushort initialList = slot.InstructionPointer;
        ushort closedBlueList = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(initialList + 2)));
        ushort activationList = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(initialList + 6)));
        ushort closedGreyDraw = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(initialList + 12)));
        ushort openTriggerList = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(activationList + 2)));
        ushort flashList = unchecked((ushort)(activationList + 8));
        ushort openingList = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(openTriggerList + 3)));
        bool wasOpened = unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasOpenedDoorBit(slot.RoomArgument);

        slot.GreyDoor = new GreyDoorPlmState(
            orientation,
            condition,
            new GreyDoorPrograms(initialList, closedBlueList, closedGreyDraw, flashList, openingList),
            wasOpened ? GreyDoorPhase.ConvertToBlue : GreyDoorPhase.Locked);
    }

    /// <summary>Maps the four native grey-door headers and Bomb Torizo's special header to their orientation.</summary>
    internal static bool TryIdentifyGreyDoor(
        PlmHeaderId header,
        out ColoredDoorOrientation orientation)
    {
        if (header == PlmHeaderId.BombTorizoGreyDoor)
        {
            orientation = ColoredDoorOrientation.Right;
            return true;
        }

        if (header is < PlmHeaderId.GreyDoorFacingLeft or
            > PlmHeaderId.GreyDoorFacingDown)
        {
            orientation = default;
            return false;
        }

        int byteOffset = header - PlmHeaderId.GreyDoorFacingLeft;
        if (byteOffset % 6 != 0 || byteOffset > 18)
        {
            orientation = default;
            return false;
        }
        orientation = ColoredDoorOrientations.AtTableIndex(byteOffset / 6);
        return true;
    }
}
