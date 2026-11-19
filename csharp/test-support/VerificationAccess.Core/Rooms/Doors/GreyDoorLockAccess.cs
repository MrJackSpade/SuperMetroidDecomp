using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Verification's view of a grey cap's lock: the condition a cap placed with it was placed with, read from what the lock
/// tests, for checks that compare it with the original's condition word.
/// </summary>
internal static class GreyDoorLockAccess
{
    /// <summary>Reads the condition a lock stands for.</summary>
    extension(GreyDoorLock doorLock)
    {
        /// <summary>The condition the cap was placed with: the one whose lock tests what this lock tests.</summary>
        /// <exception cref="InvalidOperationException">The lock tests something no placed condition stands for.</exception>
        internal GreyDoorCondition PlacedCondition => doorLock switch
        {
            GreyDoorLock.BossDefeated boss => PrivateState.Field<GreyDoorLock.BossDefeated, DefeatedBosses>(boss, "_bosses") switch
            {
                DefeatedBosses.AreaBoss => GreyDoorCondition.AreaBossDefeated,
                DefeatedBosses.AreaMiniBoss => GreyDoorCondition.AreaMiniBossDefeated,
                DefeatedBosses.AreaTorizo => GreyDoorCondition.AreaTorizoDefeated,
                var bosses => throw new InvalidOperationException($"A grey door lock waits for {bosses}, which no placed condition stands for."),
            },
            GreyDoorLock.EnemyDeathQuota => GreyDoorCondition.EnemyDeathQuota,
            GreyDoorLock.Never => GreyDoorCondition.Never,
            GreyDoorLock.TourianStatueFinished => GreyDoorCondition.TourianStatueFinished,
            GreyDoorLock.EventSet storyEvent => PrivateState.Field<GreyDoorLock.EventSet, StoryEvent>(storyEvent, "_storyEvent") switch
            {
                StoryEvent.CrittersEscaped => GreyDoorCondition.CrittersEscaped,
                var happened => throw new InvalidOperationException($"A grey door lock waits for {happened}, which no placed condition stands for."),
            },
            _ => throw new InvalidOperationException($"{doorLock.GetType().Name} stands for no placed grey door condition."),
        };
    }
}
