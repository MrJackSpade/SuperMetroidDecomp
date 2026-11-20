namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>One Atomic occupant: its own AI, with the shared reactions.</summary>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class AtomicEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => AtomicBehavior.InitializeAtomic(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => AtomicBehavior.MoveAtomicTowardSamus(Place, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => AtomicBehavior.AdvanceAtomicTypedAnimation(Place);
}
