namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Norfair Pipe Bug formation's follower (#1840, #2536): it waits for its leader to launch it.</summary>
/// <param name="species">The Pipe Bugs species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species: its speed.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class NorfairPipeBugFollowerEntity(PipeBugsBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    NorfairPipeBugSpawnSettings settings, NorfairPipeBugState state)
    : NorfairPipeBugEntity(species, occupant, place, common, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Pipe Bugs species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the follower occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species: its speed.</param>
    internal NorfairPipeBugFollowerEntity(PipeBugsBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        NorfairPipeBugSpawnSettings settings)
        : this(species, occupant, place, common, settings, species.InitializeNorfairPipeBug(place))
    {
    }

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal NorfairPipeBugSpawnSettings Settings { get; } = settings;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunNorfairPipeBugMainAi(Settings, Place, State, frame);
}
