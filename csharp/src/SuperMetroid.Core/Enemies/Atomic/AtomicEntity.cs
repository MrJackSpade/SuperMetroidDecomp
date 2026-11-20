namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>One Atomic occupant: its own AI, with the shared reactions.</summary>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class AtomicEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal AtomicEnemyState State { get; } = AtomicBehavior.InitializeAtomic(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => AtomicBehavior.RunAtomicMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => AtomicBehavior.AdvanceAtomicTypedAnimation(Place, context);
}
