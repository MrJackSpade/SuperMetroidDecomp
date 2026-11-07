using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Grey door PLMs: condition identification (boss, mini-boss, Torizo, statue, event), flash and opening.</summary>
[RoomLifetime]
internal sealed class GreyDoorObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the GreyDoor family, which has no step of its own: its slots take the shared path.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.GreyDoor, static (_, _) => RoomObjectStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>The Tourian statue sequence's progress.</summary>
    private readonly ITourianStatueProgress? _tourianStatue;
    /// <summary>Queues the dud sound for shots rejected while the door is locked.</summary>
    private readonly RoomObjectSoundQueue _sounds;
    /// <summary>The coloured-door owner whose door-bit bookkeeping grey caps share.</summary>
    private readonly DoorObjects _doors;

    /// <summary>Binds progression checks, block drawing, and PLM sound requests.</summary>
    internal GreyDoorObjects(SharedGameState? system, AreaId area, ITourianStatueProgress? tourianStatue, RoomObjectSoundQueue sounds, DoorObjects doors)
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
    /// shot wakes the cap at its hit. Each consumes the hit.
    /// </summary>
    internal void RunGreyDoorFrameBehavior(RoomObjectSlot slot, GreyDoorFrameBehavior frameBehavior, int enemyDeaths, int enemyDeathQuota)
    {
        switch (frameBehavior)
        {
            case GreyDoorFrameBehavior.WakeWhenShot:
                if (slot.ProjectileHit is null)
                    return;
                slot.ProjectileHit = null;
                slot.WakeAtLink();
                return;
            case GreyDoorFrameBehavior.PlayDudSound:
                PlayDudSoundIfShot(slot);
                return;
            case GreyDoorFrameBehavior.WakeIfAreaBossDead:
            case GreyDoorFrameBehavior.WakeIfAreaMiniBossDead:
            case GreyDoorFrameBehavior.WakeIfAreaTorizoDead:
            case GreyDoorFrameBehavior.WakeIfEnemiesDead:
            case GreyDoorFrameBehavior.WakeIfTourianStatueFinished:
            case GreyDoorFrameBehavior.WakeIfCrittersEscaped:
                if (!IsUnlocked(frameBehavior, enemyDeaths, enemyDeathQuota))
                {
                    PlayDudSoundIfShot(slot);
                    return;
                }
                if (frameBehavior == GreyDoorFrameBehavior.WakeIfEnemiesDead)
                    RequireSystem().SetEvent(EventNumber.ZebesAwake);
                slot.ProjectileHit = null;
                slot.WakeAtLink();
                return;
            case GreyDoorFrameBehavior.None:
            default:
                throw new InvalidOperationException($"Undefined {nameof(GreyDoorFrameBehavior)} {frameBehavior}.");
        }
    }

    /// <summary>A shot on a locked cap queues the dud sound; the hit is consumed either way.</summary>
    private void PlayDudSoundIfShot(RoomObjectSlot slot)
    {
        if (slot.ProjectileHit is not null)
            _sounds.Queue(Library2Sound.DudShotOnDoor, Priority: DudSoundPriority);
        slot.ProjectileHit = null;
    }

    /// <summary>The dud sound's request priority.</summary>
    private static readonly SoundRequestPriority DudSoundPriority = SoundRequestPriority.Normal;

    /// <summary>Counts a hit on a grey cap; true when it opens the door.</summary>
    internal bool CountGreyDoorHit(RoomObjectSlot slot)
    {
        GreyDoorObjectState door = (slot.Family as GreyDoorObjectState)
            ?? throw new InvalidOperationException("A grey cap's hit reached a slot without its door state.");
        door.HitCounter++;
        if (door.HitCounter < GreyDoorObjectProgramDefinitions.HitThreshold)
            return false;
        _doors.MarkDoorOpened(slot);
        return true;
    }

    /// <summary>The room's progression owner.</summary>
    private SharedGameState RequireSystem() =>
        _system ?? throw new InvalidOperationException("A resident grey door has no progression-state owner.");

    /// <summary>Whether the unlock check <paramref name="frameBehavior"/> passes.</summary>
    private bool IsUnlocked(GreyDoorFrameBehavior frameBehavior, int enemyDeaths, int enemyDeathQuota)
    {
        SharedGameState system = RequireSystem();
        return frameBehavior switch
        {
            GreyDoorFrameBehavior.WakeIfAreaBossDead => system.HasAnyBossBits(_area, BossBits.AreaBoss),
            GreyDoorFrameBehavior.WakeIfAreaMiniBossDead => system.HasAnyBossBits(_area, BossBits.AreaMiniBoss),
            GreyDoorFrameBehavior.WakeIfAreaTorizoDead => system.HasAnyBossBits(_area, BossBits.AreaTorizo),
            GreyDoorFrameBehavior.WakeIfEnemiesDead => enemyDeaths >= enemyDeathQuota,
            GreyDoorFrameBehavior.WakeIfTourianStatueFinished => _tourianStatue?.IsFinished == true,
            GreyDoorFrameBehavior.WakeIfCrittersEscaped => system.HasEvent(EventNumber.CrittersEscaped),
            GreyDoorFrameBehavior.None or
                GreyDoorFrameBehavior.WakeWhenShot or
                GreyDoorFrameBehavior.PlayDudSound => throw new InvalidOperationException($"{frameBehavior} is not an unlock check."),
            _ => throw new InvalidOperationException($"{frameBehavior} is not an unlock check."),
        };
    }

    /// <summary>Installs the resident projectile-trigger collision type while preserving the block's appearance.</summary>
    private static void ApplyGreyDoorSetup(RoomLevelData level, int blockIndex)
    {
        // Setup_GreyDoor and Setup_ColoredDoor share the exact same collision write.
        // Preserve the authored appearance while replacing only collision and behaviour.
        level.SetTerrain(blockIndex,
            level.GetCollisionBlockByIndex(blockIndex).Terrain.WithCollision(RoomCollisionType.ShootableBlock));
        level.SetBehavior(blockIndex, RoomBlockBehaviorValues.ResidentObjectProjectileTrigger);
    }

    /// <summary>
    /// Sets up a grey cap: decodes its unlock condition from the room argument, installs its shootable
    /// collision and starts its waiting program.
    /// </summary>
    internal static void SetupGreyDoorSlot(
        RoomLevelData level,
        RoomObjectSlot slot,
        ColoredDoorOrientation orientation)
    {
        GreyDoorArgument argument = slot.Argument as GreyDoorArgument ?? throw new InvalidDataException(
            $"Grey-door header {slot.Header} was placed without its condition and door bit.");
        if (argument.ConditionIndex >= GreyDoorConditions.Count)
        {
            throw new InvalidDataException(
                $"Grey-door header {slot.Header} selected invalid condition index {argument.ConditionIndex}.");
        }

        GreyDoorCondition condition = GreyDoorConditions.AtTableIndex(argument.ConditionIndex);
        slot.Argument = argument.Door;
        ApplyGreyDoorSetup(level, slot.BlockIndex);
        slot.Family = new GreyDoorObjectState(condition);
        var variant = new GreyDoorVariant(orientation, BombTorizo: slot.Header == RoomObjectHeaderId.BombTorizoGreyDoor);
        slot.StartAnimation(new GreyDoorObjectAnimation(variant, GreyDoorProgram.Waiting));
    }

    /// <summary>
    /// Maps the four grey doors and Bomb Torizo's right-facing grey door to their facing (#1414: matched by member,
    /// replacing the header range and six-byte stride the PLM headers implied).
    /// </summary>
    internal static bool TryIdentifyGreyDoor(
        RoomObjectHeaderId header,
        out ColoredDoorOrientation orientation) =>
        DoorObjectHeaderDefinitions.GreyDoors.TryGetValue(header, out orientation);
}
