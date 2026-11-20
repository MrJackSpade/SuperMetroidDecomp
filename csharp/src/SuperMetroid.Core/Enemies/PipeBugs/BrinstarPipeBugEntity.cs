namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Zeb or Zebbo (#2146), the Brinstar Pipe Bugs: it waits in its pipe, emerges and flies at Samus.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class BrinstarPipeBugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BrinstarPipeBugSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal BrinstarPipeBugState State { get; } = PipeBugsBehavior.InitializeBrinstarPipeBug(Occupant, place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunBrinstarPipeBugMainAi(Occupant, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceBrinstarPipeBugTypedAnimation(Place, State);
}
