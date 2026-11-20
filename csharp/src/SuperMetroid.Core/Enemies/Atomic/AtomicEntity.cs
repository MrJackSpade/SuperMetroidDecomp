namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>One Atomic occupant: its own AI, with the shared reactions.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class AtomicEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => AtomicBehavior.InitializeAtomic(Occupant, Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => AtomicBehavior.MoveAtomicTowardSamus(Occupant, Place, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => AtomicBehavior.AdvanceAtomicTypedAnimation(Place, Place.RequireSpeciesState<AtomicEnemyState>());
}
