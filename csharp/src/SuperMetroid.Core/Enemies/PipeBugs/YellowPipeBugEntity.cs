namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Geega (#2146), the yellow Pipe Bug: it waits, then flies straight and arcs away.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class YellowPipeBugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, YellowPipeBugSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal YellowPipeBugSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal YellowPipeBugState State { get; } = PipeBugsBehavior.InitializeYellowPipeBug(settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunYellowPipeBugMainAi(Settings, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceYellowPipeBugTypedAnimation(Place, State);
}
