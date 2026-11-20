namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Zeb or Zebbo (#2146), the Brinstar Pipe Bugs: it waits in its pipe, emerges and flies at Samus.</summary>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BrinstarPipeBugEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal BrinstarPipeBugState State { get; } = PipeBugsBehavior.InitializeBrinstarPipeBug(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunBrinstarPipeBugMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceBrinstarPipeBugTypedAnimation(Place);
}
