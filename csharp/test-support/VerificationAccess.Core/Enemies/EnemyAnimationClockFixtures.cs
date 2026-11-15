namespace SuperMetroid.Core.Enemies;

/// <summary>Seeds a fixture slot's animation clock (#1624).</summary>
internal static class EnemyAnimationClockFixtures
{
    /// <summary>
    /// Sets <paramref name="slot"/>'s clock to hold its frame for <paramref name="hold"/> updates, so a check sees what an
    /// animation start or step replaces.
    /// </summary>
    /// <returns>The same slot.</returns>
    internal static RoomEnemySlot WithAnimationClock(this RoomEnemySlot slot, int hold)
    {
        Ensure.NotNull(slot).AnimationClock.HoldFor(hold);
        return slot;
    }
}
