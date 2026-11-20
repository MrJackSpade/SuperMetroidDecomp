namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One Rio occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Rio species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RioEntity(RioBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal RioEnemyState State { get; } = RioBehavior.InitializeRio(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunRioMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RioBehavior.AdvanceRioTypedAnimation(Place, State, context);
}
