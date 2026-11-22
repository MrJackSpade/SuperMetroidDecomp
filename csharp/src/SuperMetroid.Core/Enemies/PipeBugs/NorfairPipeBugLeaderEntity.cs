namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Norfair Pipe Bug formation's leader (#1840, #2536): the Gamet its room places with a leader's settings, which leads the
/// four its room lists right after it.
/// </summary>
/// <param name="species">The Pipe Bugs species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the leader occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species: its trigger width and speed.</param>
internal sealed partial class NorfairPipeBugLeaderEntity(PipeBugsBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    NorfairPipeBugLeaderSpawnSettings settings)
    : NorfairPipeBugEntity(species, occupant, place, common)
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal NorfairPipeBugLeaderSpawnSettings Settings { get; } = settings;

    /// <summary>The formation the leader leads, which it found as it was placed.</summary>
    private NorfairPipeBugFormation Formation { get; } = species.LeadFormation(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunNorfairPipeBugLeaderMainAi(Settings, Formation, Place, State, frame);
}
