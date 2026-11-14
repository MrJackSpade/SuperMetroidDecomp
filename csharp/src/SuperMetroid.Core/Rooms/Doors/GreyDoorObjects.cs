using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What the room's grey door caps (<see cref="GreyDoorObject"/>) consult: whether a cap's condition holds (an area boss,
/// mini-boss or Torizo defeated, the room's enemy quota, the Tourian statue finished, the critters escaped, or never),
/// the dud sound a shot on a locked cap makes, and the setup that places a cap.
/// </summary>
[RoomLifetime]
internal sealed class GreyDoorObjects
{
    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>The Tourian statue sequence's progress.</summary>
    private readonly ITourianStatueProgress? _tourianStatue;
    /// <summary>Queues the dud sound for shots rejected while the door is locked.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the grey caps' conditions to the room's progression and its sound requests.</summary>
    internal GreyDoorObjects(SharedGameState? system, AreaId area, ITourianStatueProgress? tourianStatue, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _area = area;
        _tourianStatue = tourianStatue;
        _sounds = sounds;
    }

    /// <summary>Queues the dud sound a shot on a locked cap makes.</summary>
    internal void QueueDudSound() => _sounds.Queue(EnemySound.DudShotOnDoor, Priority: DudSoundPriority);

    /// <summary>The dud sound's request priority.</summary>
    private static readonly SoundRequestPriority DudSoundPriority = SoundRequestPriority.Normal;

    /// <summary>Whether <paramref name="condition"/> holds on this update; a cap that never unlocks never holds.</summary>
    internal bool IsUnlocked(GreyDoorCondition condition, RoomObjectFrameInputs frame)
    {
        SharedGameState system = RequireSystem();
        return condition switch
        {
            GreyDoorCondition.AreaBossDefeated => system.HasDefeatedAnyOf(_area, DefeatedBosses.AreaBoss),
            GreyDoorCondition.AreaMiniBossDefeated => system.HasDefeatedAnyOf(_area, DefeatedBosses.AreaMiniBoss),
            GreyDoorCondition.AreaTorizoDefeated => system.HasDefeatedAnyOf(_area, DefeatedBosses.AreaTorizo),
            GreyDoorCondition.EnemyDeathQuota => frame.EnemyDeaths >= frame.EnemyDeathQuota,
            GreyDoorCondition.Never => false,
            GreyDoorCondition.TourianStatueFinished => _tourianStatue?.IsFinished == true,
            GreyDoorCondition.CrittersEscaped => system.HasEvent(StoryEvent.CrittersEscaped),
            _ => throw new InvalidOperationException($"Undefined {nameof(GreyDoorCondition)} {condition}."),
        };
    }

    /// <summary>What a cap's condition holding does besides unlocking it: the room's enemy quota wakes Zebes.</summary>
    internal void RecordUnlocked(GreyDoorCondition condition)
    {
        if (condition == GreyDoorCondition.EnemyDeathQuota)
            RequireSystem().SetEvent(StoryEvent.ZebesAwake);
    }

    /// <summary>The room's progression owner.</summary>
    private SharedGameState RequireSystem() =>
        _system ?? throw new InvalidOperationException("A resident grey door has no progression-state owner.");

    /// <summary>Installs the resident projectile-trigger collision type while preserving the block's appearance.</summary>
    private static void ApplyGreyDoorSetup(RoomLevelData level, Position16<RoomBlocks> block)
    {
        // Grey and coloured doors' setups share the exact same collision write.
        // Preserve the authored appearance while replacing only collision and behaviour.
        level.SetTerrain(block,
            level.GetCollisionBlock(block).Terrain.WithCollision(RoomCollisionType.ShootableBlock));
        level.SetBehavior(block, RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger));
    }

    /// <summary>
    /// Sets up the grey cap <paramref name="variant"/> the room's population placed: takes its unlock condition from the
    /// room argument, installs its shootable collision and places the cap, which takes its first look on its first update.
    /// </summary>
    internal static void SetupGreyDoorSlot(
        RoomLevelData level,
        RoomObjectSlot slot,
        GreyDoorVariant variant)
    {
        Ensure.NotNull(slot);
        GreyDoorArgument argument = slot.Argument as GreyDoorArgument ?? throw new InvalidDataException(
            $"Grey door {variant} at block {slot.Block} was placed without its condition and door bit.");
        slot.Argument = argument.Door;
        ApplyGreyDoorSetup(level, slot.Block);
        slot.Become(new GreyDoorObject(variant, argument.Condition));
    }

    /// <summary>
    /// Maps the four grey doors and Bomb Torizo's right-facing grey door to their facing (#1414: matched by member,
    /// replacing the range the room-object definitions implied).
    /// </summary>
    internal static bool TryIdentifyGreyDoor(
        RoomObjectKind kind,
        out ColoredDoorOrientation orientation) =>
        DoorObjectKindDefinitions.GreyDoors.TryGetValue(kind, out orientation);
}
