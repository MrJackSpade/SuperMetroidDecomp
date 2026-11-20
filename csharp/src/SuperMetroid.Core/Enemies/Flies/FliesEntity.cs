namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>One Flies occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Flies species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FliesEntity(FliesBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal FlyEnemyState State { get; } = FliesBehavior.InitializeFly(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunFliesMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => FliesBehavior.AdvanceFliesTypedAnimation(Place, State, context);
}
