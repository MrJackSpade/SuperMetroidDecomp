namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Geega (#2146), the yellow Pipe Bug: it waits, then flies straight and arcs away.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class YellowPipeBugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    YellowPipeBugSpawnSettings settings, YellowPipeBugState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the Pipe Bug occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    internal YellowPipeBugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        YellowPipeBugSpawnSettings settings)
        : this(occupant, place, common, settings, PipeBugsBehavior.InitializeYellowPipeBug(settings, place))
    {
    }

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal YellowPipeBugSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly YellowPipeBugState state = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunYellowPipeBugMainAi(Settings, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceYellowPipeBugTypedAnimation(Place, state);
}
