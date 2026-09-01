using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored condition gates for bank-$84's grey door family.</summary>
public sealed partial class RoomPlmSystem
{
    private const ushort BombTorizoGreyDoorHeader = 0xbaf4;
    private const ushort GreyDoorFacingLeftHeader = 0xc842;
    private const ushort GreyDoorFacingRightHeader = 0xc848;
    private const ushort GreyDoorFacingUpHeader = 0xc84e;
    private const ushort GreyDoorFacingDownHeader = 0xc854;

    private Bank80SystemState? _greyDoorSystem;
    private byte _greyDoorAreaIndex;
    private Func<bool>? _isTourianStatueFinished;

    /// <summary>Debugger-stable views of every resident grey-door PLM.</summary>
    public IReadOnlyList<GreyDoorPlmSnapshot> GreyDoors => _slots
        .Where(slot => slot.Active && slot.GreyDoor is not null)
        .Select(slot => new GreyDoorPlmSnapshot(
            slot.HeaderPointer,
            slot.BlockIndex,
            slot.RoomArgument,
            slot.GreyDoor!.Orientation,
            slot.GreyDoor.Condition,
            slot.GreyDoor.Phase,
            slot.GreyDoor.InitialList,
            slot.GreyDoor.FlashList,
            slot.GreyDoor.OpeningList))
        .ToArray();

    /// <summary>
    /// Runs setup <c>$84:C794</c> and allocates every generic or Bomb-Torizo grey door in
    /// one room's bank-$8F PLM population.
    /// </summary>
    /// <remarks>
    /// Nothing in this loader identifies a particular room. The condition comes from the
    /// high bits of the cartridge room argument, and every visual/list pointer is followed
    /// from the selected bank-$84 header. That distinction matters for Old Mother Brain:
    /// its enemy-quota door is the native producer of event zero (Zebes awake), so setting
    /// that event in Bomb Torizo's death code would both bypass the door and incorrectly
    /// make Golden Torizo publish Crateria progression.
    /// </remarks>
    public int LoadGreyDoorPopulation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        ushort populationPointer,
        Bank80SystemState system,
        byte areaIndex,
        Func<bool>? isTourianStatueFinished = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(system);

        _greyDoorSystem = system;
        _greyDoorAreaIndex = areaIndex;
        _isTourianStatueFinished = isTourianStatueFinished;

        int loaded = 0;
        ushort cursor = populationPointer;
        for (int recordIndex = 0; recordIndex < 256; recordIndex++)
        {
            ushort header = ReadBank8fWord(bus, cursor);
            if (header == 0)
                return loaded;

            byte blockX = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 2)));
            byte blockY = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 3)));
            ushort rawRoomArgument = ReadBank8fWord(
                bus,
                unchecked((ushort)(cursor + 4)));
            cursor = unchecked((ushort)(cursor + 6));

            if (!TryIdentifyGreyDoor(header, out ColoredDoorOrientation orientation))
                continue;

            // Setup_GreyDoor reads the high argument byte, masks bits 2..6, and shifts
            // once. Expressing the result as a typed table index prevents the low ten
            // persistence bits from being mistaken for flags belonging to the condition.
            int conditionOffset = ((rawRoomArgument >> 8) & 0x7c) >> 1;
            if ((conditionOffset & 1) != 0 || conditionOffset > 12)
            {
                throw new InvalidDataException(
                    $"Grey-door header ${header:X4} selected invalid condition offset " +
                    $"${conditionOffset:X2} from room argument ${rawRoomArgument:X4}.");
            }
            GreyDoorCondition condition = (GreyDoorCondition)(conditionOffset >> 1);

            // `$84:C79E-$C7A4` removes the condition selector while retaining bit 15's
            // negative/no-persistence sentinel and the ten-bit opened-door bit index.
            ushort roomArgument = unchecked((ushort)(rawRoomArgument & 0x83ff));
            int blockIndex = level.GetBlockIndex(blockX, blockY);
            ApplyGreyDoorSetup(level, blockIndex);

            PlmSlot? slot = AllocateGreyDoorSlot();
            if (slot is null)
                continue;

            // The four orientations and the special Bomb Torizo header share the same
            // graph shape. Following its operands keeps alternate compatible ROM data
            // authoritative without replacing animation pointers with host constants.
            ushort initialList = ReadBank84Word(bus, unchecked((ushort)(header + 2)));
            ushort closedBlueList = ReadBank84Word(
                bus,
                unchecked((ushort)(initialList + 2)));
            ushort activationList = ReadBank84Word(
                bus,
                unchecked((ushort)(initialList + 6)));
            ushort closedGreyDraw = ReadBank84Word(
                bus,
                unchecked((ushort)(initialList + 12)));
            ushort openTriggerList = ReadBank84Word(
                bus,
                unchecked((ushort)(activationList + 2)));
            ushort flashList = unchecked((ushort)(activationList + 8));
            ushort openingList = ReadBank84Word(
                bus,
                unchecked((ushort)(openTriggerList + 3)));

            bool wasOpened = unchecked((short)roomArgument) >= 0 &&
                system.HasOpenedDoorBit(roomArgument);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = initialList;
            slot.InstructionTimer = 1;
            slot.PreInstruction = 0;
            slot.RoomArgument = roomArgument;
            slot.LoopTimer = 0;
            slot.Item = null;
            slot.Scroll = null;
            slot.ColoredDoor = null;
            slot.GreyDoor = new GreyDoorPlmState(
                orientation,
                condition,
                initialList,
                closedBlueList,
                closedGreyDraw,
                flashList,
                openingList,
                wasOpened ? GreyDoorPhase.ConvertToBlue : GreyDoorPhase.Locked);
            loaded++;
        }

        throw new InvalidDataException(
            $"Room PLM population $8F:{populationPointer:X4} has no zero terminator.");
    }

    /// <summary>Publishes the native projectile word to a resident grey-door actor.</summary>
    private bool TryNotifyGreyDoorHit(int blockIndex, ushort projectileType)
    {
        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.GreyDoor is null)
                continue;

            // Once the one-hit instruction selects the opening stream, native clears the
            // shot pre-instruction. Further impacts cannot restart or duplicate the sound.
            if (slot.GreyDoor.Phase is GreyDoorPhase.Opening or GreyDoorPhase.ConvertToBlue)
                return false;

            slot.GreyDoor.PendingProjectileType = projectileType;
            slot.GreyDoor.HasPendingHit = true;
            return true;
        }
        return false;
    }

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

        if (door.Phase == GreyDoorPhase.ConvertToBlue)
        {
            // Goto_if_room_argument_door_is_set selects the ordinary closed-blue list.
            // Its PLM_BTS_Y opcode and single draw are observable immediately on reload.
            byte blueBts = unchecked((byte)(BlueDoorFacingLeftBts + (byte)door.Orientation));
            ushort drawPointer = ReadBank84Word(
                bus,
                unchecked((ushort)(door.ClosedBlueList + 5)));
            level.SetBehavior(slot.BlockIndex, blueBts);
            DrawRomInstruction(
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
            DrawRomInstruction(
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
                    _soundRequests.Add(new PlmSoundRequest(2, 0x57, MaximumQueued: 6));
                door.HasPendingHit = false;
                door.PendingProjectileType = 0;
                return true;
            }

            if (door.Condition == GreyDoorCondition.EnemyDeathQuota)
            {
                (_greyDoorSystem ?? throw new InvalidOperationException(
                    "A resident grey door has no progression-state owner."))
                    .SetEvent((int)EventNumber.ZebesAwake);
            }

            // Goto_Link_Instruction clears shot status, sets timer one, and selects the
            // activation link. That link installs the later shot link/pre-instruction and
            // falls straight into the repeating flash list on this same PLM handler pass.
            // Clearing the hit here is essential: the quota-completing frame cannot also
            // open the door even if a projectile happened to collide during that frame.
            door.HasPendingHit = false;
            door.PendingProjectileType = 0;
            door.Phase = GreyDoorPhase.Flashing;
            slot.InstructionPointer = door.FlashList;
            slot.InstructionTimer = 1;
            slot.LoopTimer = 0;
            return false;
        }

        if (door.Phase == GreyDoorPhase.Flashing && door.HasPendingHit)
        {
            door.HasPendingHit = false;
            door.PendingProjectileType = 0;
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
                system.HasAnyBossBits(_greyDoorAreaIndex, BossBits.AreaBoss),
            GreyDoorCondition.AreaMiniBossDefeated =>
                system.HasAnyBossBits(_greyDoorAreaIndex, BossBits.AreaMiniBoss),
            GreyDoorCondition.AreaTorizoDefeated =>
                system.HasAnyBossBits(_greyDoorAreaIndex, BossBits.AreaTorizo),
            GreyDoorCondition.EnemyDeathQuota => enemyDeaths >= enemyDeathQuota,
            GreyDoorCondition.Never => false,
            GreyDoorCondition.TourianStatueFinished =>
                _isTourianStatueFinished?.Invoke() == true,
            GreyDoorCondition.CrittersEscaped =>
                system.HasEvent((int)EventNumber.CrittersEscaped),
            _ => throw new InvalidDataException(
                $"Unknown grey-door condition {(byte)condition}.")
        };
    }

    private static void ApplyGreyDoorSetup(RoomLevelData level, int blockIndex)
    {
        // Setup_GreyDoor and Setup_ColoredDoor share the exact `$C044` collision write.
        // Preserve the authored visual payload while replacing only type and BTS.
        ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetForegroundEntry(blockIndex, unchecked((ushort)((originalWord & 0x0fff) | 0xc000)));
        level.SetBehavior(blockIndex, 0x44);
    }

    private PlmSlot? AllocateGreyDoorSlot()
    {
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            if (!_slots[index].Active)
                return _slots[index];
        }
        return null;
    }

    private static bool TryIdentifyGreyDoor(
        ushort header,
        out ColoredDoorOrientation orientation)
    {
        if (header == BombTorizoGreyDoorHeader)
        {
            orientation = ColoredDoorOrientation.Right;
            return true;
        }

        if (header is < GreyDoorFacingLeftHeader or > GreyDoorFacingDownHeader)
        {
            orientation = default;
            return false;
        }

        int byteOffset = header - GreyDoorFacingLeftHeader;
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
    AreaBossDefeated,
    AreaMiniBossDefeated,
    AreaTorizoDefeated,
    EnemyDeathQuota,
    Never,
    TourianStatueFinished,
    CrittersEscaped,
}

/// <summary>Debugger-visible phase of one resident grey-door PLM.</summary>
public enum GreyDoorPhase : byte
{
    Locked,
    Flashing,
    Opening,
    ConvertToBlue,
}

/// <summary>Stable debugger view over a resident grey-door PLM slot.</summary>
public readonly record struct GreyDoorPlmSnapshot(
    ushort Header,
    int BlockIndex,
    ushort RoomArgument,
    ColoredDoorOrientation Orientation,
    GreyDoorCondition Condition,
    GreyDoorPhase Phase,
    ushort InitialList,
    ushort FlashList,
    ushort OpeningList);

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
    public ColoredDoorOrientation Orientation { get; } = orientation;
    public GreyDoorCondition Condition { get; } = condition;
    public ushort InitialList { get; } = initialList;
    public ushort ClosedBlueList { get; } = closedBlueList;
    public ushort ClosedGreyDraw { get; } = closedGreyDraw;
    public ushort FlashList { get; } = flashList;
    public ushort OpeningList { get; } = openingList;
    public GreyDoorPhase Phase { get; set; } = phase;
    public bool InitialDrawCompleted { get; set; }
    public bool HasPendingHit { get; set; }
    public ushort PendingProjectileType { get; set; }
}
