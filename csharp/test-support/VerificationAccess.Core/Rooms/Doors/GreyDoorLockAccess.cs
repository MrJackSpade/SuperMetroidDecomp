using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Verification's view of a grey cap's lock: the condition a cap placed with it was placed with, read from what the lock
/// tests, for checks that compare it with the original's condition word.
/// </summary>
internal static class GreyDoorLockAccess
{
    /// <summary>The condition each defeated-boss lock stands for, by the bosses it waits for.</summary>
    private static readonly Dictionary<DefeatedBosses, GreyDoorCondition> BossConditions = new()
    {
        [DefeatedBosses.AreaBoss] = GreyDoorCondition.AreaBossDefeated,
        [DefeatedBosses.AreaMiniBoss] = GreyDoorCondition.AreaMiniBossDefeated,
        [DefeatedBosses.AreaTorizo] = GreyDoorCondition.AreaTorizoDefeated,
    };

    /// <summary>The condition each story-event lock stands for, by the event it waits for.</summary>
    private static readonly Dictionary<StoryEvent, GreyDoorCondition> EventConditions = new()
    {
        [StoryEvent.CrittersEscaped] = GreyDoorCondition.CrittersEscaped,
    };

    /// <summary>Reads the condition a lock stands for.</summary>
    extension(GreyDoorLock doorLock)
    {
        /// <summary>The condition the cap was placed with: the one whose lock tests what this lock tests.</summary>
        /// <exception cref="InvalidOperationException">The lock is of a kind no placed condition stands for.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The lock waits for bosses or an event no placed condition stands for.</exception>
        internal GreyDoorCondition PlacedCondition => doorLock switch
        {
            GreyDoorLock.BossDefeated boss => Ensure.Listed(BossConditions,
                PrivateState.Field<GreyDoorLock.BossDefeated, DefeatedBosses>(boss, "_bosses")),
            GreyDoorLock.EnemyDeathQuota => GreyDoorCondition.EnemyDeathQuota,
            GreyDoorLock.Never => GreyDoorCondition.Never,
            GreyDoorLock.TourianStatueFinished => GreyDoorCondition.TourianStatueFinished,
            GreyDoorLock.EventSet storyEvent => Ensure.Listed(EventConditions,
                PrivateState.Field<GreyDoorLock.EventSet, StoryEvent>(storyEvent, "_storyEvent")),
            _ => throw new InvalidOperationException($"{doorLock.GetType().Name} stands for no placed grey door condition."),
        };
    }
}
