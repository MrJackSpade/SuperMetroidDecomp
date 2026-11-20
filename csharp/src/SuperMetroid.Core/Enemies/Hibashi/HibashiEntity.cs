namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>A Hibashi's visible flame: its rest, eruption and the collision part it moves, with the shared reactions.</summary>
/// <param name="species">The Hibashi species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HibashiEntity(HibashiBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal HibashiEnemyState State { get; } = species.InitializeHibashi(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunHibashiMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceHibashiTypedAnimation(Place, State, context);
}
