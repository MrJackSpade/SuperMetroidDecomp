using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Grey door PLMs: condition identification (boss, mini-boss, Torizo, statue, event), flash and opening.</summary>
[RoomLifetime]
internal sealed class GreyDoorPlms : IPlmFamilySource
{
    /// <summary>Registers the GreyDoor family, which has no step of its own: its slots take the shared path.</summary>
    public void AddPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Register(PlmFamily.GreyDoor, static (_, _) => PlmStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly Bank80SystemState? _system;
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>The Tourian statue sequence's progress.</summary>
    private readonly ITourianStatueProgress? _tourianStatue;
    /// <summary>Queues the cartridge's dud sound for shots rejected while the door is locked.</summary>
    private readonly PlmSoundQueue _sounds;
    /// <summary>The coloured-door owner whose door-bit bookkeeping grey caps share.</summary>
    private readonly DoorPlms _doors;

    /// <summary>Binds progression checks, block drawing, and PLM sound requests.</summary>
    internal GreyDoorPlms(Bank80SystemState? system, AreaId area, ITourianStatueProgress? tourianStatue, PlmSoundQueue sounds, DoorPlms doors)
    {
        _doors = doors;
        _system = system;
        _area = area;
        _tourianStatue = tourianStatue;
        _sounds = sounds;
    }

    /// <summary>
    /// The grey cap's pre-instructions: an unlock check wakes the cap at its activation once the door's condition holds
    /// (setting Zebes awake for the enemy quota), and otherwise plays the dud sound for a shot; once activated, any
    /// shot wakes the cap at its hit. Each consumes the hit word.
    /// </summary>
    internal void RunGreyDoorPreInstruction(PlmSlot slot, GreyDoorPreInstruction preInstruction, int enemyDeaths, int enemyDeathQuota)
    {
        switch (preInstruction)
        {
            case GreyDoorPreInstruction.WakeWhenShot:
                if (slot.LoopTimer == 0)
                    return;
                slot.LoopTimer = 0;
                slot.WakeAtLink();
                return;
            case GreyDoorPreInstruction.PlayDudSound:
                PlayDudSoundIfShot(slot);
                return;
            case GreyDoorPreInstruction.WakeIfAreaBossDead:
            case GreyDoorPreInstruction.WakeIfAreaMiniBossDead:
            case GreyDoorPreInstruction.WakeIfAreaTorizoDead:
            case GreyDoorPreInstruction.WakeIfEnemiesDead:
            case GreyDoorPreInstruction.WakeIfTourianStatueFinished:
            case GreyDoorPreInstruction.WakeIfCrittersEscaped:
                if (!IsUnlocked(preInstruction, enemyDeaths, enemyDeathQuota))
                {
                    PlayDudSoundIfShot(slot);
                    return;
                }
                if (preInstruction == GreyDoorPreInstruction.WakeIfEnemiesDead)
                    RequireSystem().SetEvent(EventNumber.ZebesAwake);
                slot.LoopTimer = 0;
                slot.WakeAtLink();
                return;
            case GreyDoorPreInstruction.None:
            default:
                throw new InvalidOperationException($"Undefined {nameof(GreyDoorPreInstruction)} {preInstruction}.");
        }
    }

    /// <summary><c>$84:BDC4</c>: a shot on a locked cap queues the dud sound; the hit word is consumed either way.</summary>
    private void PlayDudSoundIfShot(PlmSlot slot)
    {
        if (slot.LoopTimer != 0)
            _sounds.Queue(Library2Sound.DudShotOnDoor, MaximumQueued: DudSoundMaximumQueued);
        slot.LoopTimer = 0;
    }

    /// <summary>The dud sound's native queue cap.</summary>
    private static readonly SoundQueueLimit DudSoundMaximumQueued = SoundQueueLimit.Max6;

    /// <summary><c>$84:8A91</c>: counts a hit on a grey cap; true when it opens the door.</summary>
    internal bool CountGreyDoorHit(PlmSlot slot)
    {
        GreyDoorPlmState door = (slot.Family as GreyDoorPlmState)
            ?? throw new InvalidOperationException("A grey cap's hit reached a slot without its door state.");
        door.HitCounter++;
        if (door.HitCounter < GreyDoorPlmProgramDefinitions.HitThreshold)
            return false;
        _doors.MarkDoorOpened(slot);
        return true;
    }

    /// <summary>The room's progression owner.</summary>
    private Bank80SystemState RequireSystem() =>
        _system ?? throw new InvalidOperationException("A resident grey door has no progression-state owner.");

    /// <summary>Whether the unlock check <paramref name="preInstruction"/> passes.</summary>
    private bool IsUnlocked(GreyDoorPreInstruction preInstruction, int enemyDeaths, int enemyDeathQuota)
    {
        Bank80SystemState system = RequireSystem();
        return preInstruction switch
        {
            GreyDoorPreInstruction.WakeIfAreaBossDead => system.HasAnyBossBits(_area, BossBits.AreaBoss),
            GreyDoorPreInstruction.WakeIfAreaMiniBossDead => system.HasAnyBossBits(_area, BossBits.AreaMiniBoss),
            GreyDoorPreInstruction.WakeIfAreaTorizoDead => system.HasAnyBossBits(_area, BossBits.AreaTorizo),
            GreyDoorPreInstruction.WakeIfEnemiesDead => enemyDeaths >= enemyDeathQuota,
            GreyDoorPreInstruction.WakeIfTourianStatueFinished => _tourianStatue?.IsFinished == true,
            GreyDoorPreInstruction.WakeIfCrittersEscaped => system.HasEvent(EventNumber.CrittersEscaped),
            GreyDoorPreInstruction.None or
                GreyDoorPreInstruction.WakeWhenShot or
                GreyDoorPreInstruction.PlayDudSound => throw new InvalidOperationException($"{preInstruction} is not an unlock check."),
            _ => throw new InvalidOperationException($"{preInstruction} is not an unlock check."),
        };
    }

    /// <summary>Installs the resident projectile-trigger collision type while preserving the block's visual bits.</summary>
    private static void ApplyGreyDoorSetup(RoomLevelData level, int blockIndex)
    {
        // Setup_GreyDoor and Setup_ColoredDoor share the exact `$C044` collision write.
        // Preserve the authored visual payload while replacing only type and BTS.
        ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw;
        level.SetForegroundEntry(blockIndex, new RoomLevelWord(originalWord).WithCollisionType(RoomCollisionType.ShootableBlock).Raw);
        level.SetBehavior(blockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
    }

    /// <summary>
    /// Setup <c>$84:C794</c> for a grey cap: decodes its unlock condition from the room argument, installs its shootable
    /// collision and starts its waiting program.
    /// </summary>
    internal static void SetupGreyDoorSlot(
        RoomLevelData level,
        PlmSlot slot,
        ColoredDoorOrientation orientation)
    {
        int rawRoomArgument = slot.RoomArgument;
        int conditionOffset = GreyDoorRoomArgumentDefinitions.ConditionOffset(rawRoomArgument);
        if ((conditionOffset & 1) != 0 || conditionOffset > 12)
        {
            throw new InvalidDataException(
                $"Grey-door header ${(int)slot.Header:X4} selected invalid condition " +
                $"offset ${conditionOffset:X2} from room argument ${rawRoomArgument:X4}.");
        }

        GreyDoorCondition condition = GreyDoorConditions.AtTableIndex(conditionOffset >> 1);
        slot.RoomArgument = GreyDoorRoomArgumentDefinitions.DoorBitArgument(rawRoomArgument);
        ApplyGreyDoorSetup(level, slot.BlockIndex);
        slot.Family = new GreyDoorPlmState(condition);
        var variant = new GreyDoorVariant(orientation, BombTorizo: slot.Header == PlmHeaderId.BombTorizoGreyDoor);
        slot.StartAnimation(new GreyDoorPlmAnimation(variant, GreyDoorProgram.Waiting));
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
