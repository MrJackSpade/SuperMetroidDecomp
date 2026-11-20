using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What the room's grey door caps (<see cref="GreyDoorObject"/>) consult: the dud sound a shot on a locked cap makes, and
/// the setup that places a cap with its lock (<see cref="GreyDoorLock"/>): an area boss, mini-boss or Torizo defeated, the
/// room's enemy quota, the Tourian statue finished, the critters escaped, or never.
/// </summary>
[RoomLifetime]
internal sealed class GreyDoorObjects
{
    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState _system;
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>Whether the room's Tourian statue has finished sinking, which a cap may wait for (#2486, #2554).</summary>
    private readonly ITourianStatueProgress _tourianStatue;
    /// <summary>Queues the dud sound for shots rejected while the door is locked.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the grey caps' conditions to the room's progression and its sound requests.</summary>
    /// <param name="system">The game's progress, whose bosses and events the caps' locks read.</param>
    /// <param name="area">The room's area, whose bosses the caps wait for.</param>
    /// <param name="tourianStatue">Whether the room's Tourian statue has finished sinking, which only the statue room's statues finish.</param>
    /// <param name="sounds">The room objects' sound requests.</param>
    internal GreyDoorObjects(SharedGameState system, AreaId area, ITourianStatueProgress tourianStatue, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _area = area;
        _tourianStatue = Ensure.NotNull(tourianStatue);
        _sounds = sounds;
    }

    /// <summary>Queues the dud sound a shot on a locked cap makes.</summary>
    internal void QueueDudSound() => _sounds.Queue(EnemySound.DudShot, Priority: DudSoundPriority);

    /// <summary>The dud sound's request priority.</summary>
    private static readonly SoundRequestPriority DudSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The lock a cap placed with <paramref name="condition"/> waits on, with what its test reads.</summary>
    private GreyDoorLock LockFor(GreyDoorCondition condition) => condition switch
    {
        GreyDoorCondition.AreaBossDefeated => new GreyDoorLock.BossDefeated(_system, _area, DefeatedBosses.AreaBoss),
        GreyDoorCondition.AreaMiniBossDefeated => new GreyDoorLock.BossDefeated(_system, _area, DefeatedBosses.AreaMiniBoss),
        GreyDoorCondition.AreaTorizoDefeated => new GreyDoorLock.BossDefeated(_system, _area, DefeatedBosses.AreaTorizo),
        GreyDoorCondition.EnemyDeathQuota => new GreyDoorLock.EnemyDeathQuota(_system),
        GreyDoorCondition.Never => new GreyDoorLock.Never(),
        GreyDoorCondition.TourianStatueFinished => new GreyDoorLock.TourianStatueFinished(_tourianStatue),
        GreyDoorCondition.CrittersEscaped => new GreyDoorLock.EventSet(_system, StoryEvent.CrittersEscaped),
        _ => throw new InvalidDataException($"Undefined {nameof(GreyDoorCondition)} {condition}."),
    };

    /// <summary>Installs the resident projectile-trigger collision type while preserving the block's appearance.</summary>
    private static void ApplyGreyDoorSetup(RoomLevelData level, Position16<RoomBlocks> block)
    {
        // A grey door's block becomes shootable and solid, as a coloured door's does, and keeps its picture and what it
        // shows (#2502).
        level.SetCollision(block, RoomCollisionType.ShootableBlock);
        level.SetBehavior(block, RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger));
    }

    /// <summary>
    /// Sets up the grey cap <paramref name="variant"/> the room's population placed with <paramref name="placed"/>: its unlock
    /// condition, resolved into its lock, and its door. Installs its shootable collision and places the cap, which takes its
    /// first look on its first update.
    /// </summary>
    internal void SetupGreyDoor(
        RoomLevelData level,
        RoomObjectPlace place,
        GreyDoorVariant variant,
        GreyDoorOpening placed)
    {
        Ensure.NotNull(place);
        Ensure.NotNull(placed);
        GreyDoorLock doorLock = LockFor(placed.Condition);
        ApplyGreyDoorSetup(level, place.Block);
        place.BecomeDoor(new GreyDoorObject(variant, doorLock, new PlacedDoor(placed.Door)));
    }
}
