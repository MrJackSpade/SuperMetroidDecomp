namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One Hoppers occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Hoppers species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HoppersEntity(HoppersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal HopperEnemyState State { get; } = species.InitializeHopper(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunHoppersMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceHoppersTypedAnimation(Place, context);
}
