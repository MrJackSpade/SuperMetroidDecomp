namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Gamet (#2146), the Norfair Pipe Bug: it rises with its formation, staggers and flies at Samus.</summary>
/// <param name="species">The Pipe Bugs species owner.</param>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairPipeBugEntity(PipeBugsBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal NorfairPipeBugState State { get; } = species.InitializeNorfairPipeBug(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairPipeBugMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceNorfairPipeBugTypedAnimation(Place);
}
