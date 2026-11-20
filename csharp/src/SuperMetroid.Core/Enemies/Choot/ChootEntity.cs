namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One Choot occupant: its own AI, with the shared reactions.</summary>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChootEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal ChootEnemyState State { get; } = ChootBehavior.InitializeChoot(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ChootBehavior.RunChootMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ChootBehavior.AdvanceChootTypedAnimation(Place, context);
}
