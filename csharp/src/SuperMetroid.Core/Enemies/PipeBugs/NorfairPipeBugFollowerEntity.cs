namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Norfair Pipe Bug formation's follower (#1840, #2536): it waits for its leader to launch it.</summary>
/// <param name="species">The Pipe Bugs species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species: its speed.</param>
internal sealed partial class NorfairPipeBugFollowerEntity(PipeBugsBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    NorfairPipeBugSpawnSettings settings)
    : NorfairPipeBugEntity(species, occupant, place, common)
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal NorfairPipeBugSpawnSettings Settings { get; } = settings;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunNorfairPipeBugMainAi(Settings, Place, State, frame);
}
