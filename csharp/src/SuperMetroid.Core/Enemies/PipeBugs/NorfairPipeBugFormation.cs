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
    /// The formation <paramref name="leader"/> leads: itself and the four its room lists with it in the formation's roles
    /// (<see cref="PipeBugDefinitions.Formation"/>).
    /// </summary>
    /// <param name="places">The room's enemy places.</param>
    /// <param name="leader">The formation's leader.</param>
    internal static NorfairPipeBugFormation LedBy(EnemyPlaces places, RoomEnemyPlace leader) =>
        new([.. PipeBugDefinitions.Formation.ListedRoles.Select(role => new NorfairPipeBugMember(
            role == PipeBugDefinitions.LeaderRole ? leader : places.Member(leader, PipeBugDefinitions.Formation, PipeBugDefinitions.LeaderRole, role),
            role))]);
}
