namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Geega (#2146), the yellow Pipe Bug: it waits, then flies straight and arcs away.</summary>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YellowPipeBugEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal YellowPipeBugState State { get; } = PipeBugsBehavior.InitializeYellowPipeBug(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunYellowPipeBugMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceYellowPipeBugTypedAnimation(Place);
}
