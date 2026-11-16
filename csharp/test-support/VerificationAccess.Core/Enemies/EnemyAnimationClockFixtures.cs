namespace SuperMetroid.Core.Enemies;

/// <summary>Seeds a fixture slot's animation clock (#1624).</summary>
internal static class EnemyAnimationClockFixtures
{
    /// <summary>
    /// Sets <paramref name="place"/>'s clock to hold its frame for <paramref name="hold"/> updates, so a check sees what an
    /// animation start or step replaces.
    /// </summary>
    /// <returns>The same slot.</returns>
    internal static RoomEnemyPlace WithAnimationClock(this RoomEnemyPlace place, int hold)
    {
        Ensure.NotNull(place).AnimationClock.HoldFor(hold);
        return place;
    }
}
