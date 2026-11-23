namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Zeb or Zebbo (#2146), the Brinstar Pipe Bugs: it waits in its pipe, emerges and flies at Samus.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class BrinstarPipeBugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    BrinstarPipeBugSpawnSettings settings, BrinstarPipeBugState state)
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
    internal BrinstarPipeBugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        BrinstarPipeBugSpawnSettings settings)
        : this(occupant, place, common, settings, PipeBugsBehavior.InitializeBrinstarPipeBug(settings, place))
    {
    }

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal BrinstarPipeBugSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal BrinstarPipeBugState State { get; } = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunBrinstarPipeBugMainAi(Settings, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceBrinstarPipeBugTypedAnimation(Place, State);
}
