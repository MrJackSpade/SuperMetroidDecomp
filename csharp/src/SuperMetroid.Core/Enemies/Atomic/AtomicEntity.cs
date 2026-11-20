namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>One Atomic occupant: its own AI, with the shared reactions.</summary>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class AtomicEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, AtomicSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal AtomicEnemyState State { get; } = AtomicBehavior.InitializeAtomic(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => AtomicBehavior.RunAtomicMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => AtomicBehavior.AdvanceAtomicTypedAnimation(Place, State, context);
}
