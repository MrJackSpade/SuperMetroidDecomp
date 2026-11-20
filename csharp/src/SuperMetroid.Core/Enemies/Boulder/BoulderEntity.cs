namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>One Boulder occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Boulder species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BoulderEntity(BoulderBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal BoulderEnemyState State { get; } = BoulderBehavior.InitializeBoulder(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBoulderMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BoulderBehavior.AdvanceBoulderTypedAnimation(Place, context);
}
