namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Norfair Pipe Bug formation (#1840): its leader, the Pipe Bug its room places with leader's settings
/// (<see cref="NorfairPipeBugLeaderSpawnSettings"/>, #2536), and the four its room lists right after it, each in its own role: the near and far members above the leader, then the near and far members
/// below it.
/// </summary>
/// <param name="Members">The members, the leader first, each with its role.</param>
internal sealed record NorfairPipeBugFormation(IReadOnlyList<NorfairPipeBugMember> Members)
{
    /// <summary>
    /// The formation <paramref name="leader"/> leads: itself and the four places its room lists right after it, which take
    /// the upper near, upper far, lower near and lower far roles in that order.
    /// </summary>
    /// <param name="places">The room's enemy places.</param>
    /// <param name="leader">The formation's leader.</param>
    internal static NorfairPipeBugFormation LedBy(EnemyPlaces places, RoomEnemyPlace leader)
    {
        RoomEnemyPlace[] followers = places.ListedRunAfter(leader, 4);
        return new NorfairPipeBugFormation(
        [
            new(leader, PipeBugDefinitions.LeaderRole),
            new(followers[0], PipeBugDefinitions.UpperNearRole),
            new(followers[1], PipeBugDefinitions.UpperFarRole),
            new(followers[2], PipeBugDefinitions.LowerNearRole),
            new(followers[3], PipeBugDefinitions.LowerFarRole),
        ]);
    }
}
