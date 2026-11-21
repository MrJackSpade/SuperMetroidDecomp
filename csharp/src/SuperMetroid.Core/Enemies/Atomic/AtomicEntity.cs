namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>One Atomic occupant: its own AI, with the shared reactions.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class AtomicEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, AtomicSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal AtomicSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly AtomicEnemyState state = AtomicBehavior.InitializeAtomic(settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => AtomicBehavior.MoveAtomicTowardSamus(Settings, Place, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => AtomicBehavior.AdvanceAtomicTypedAnimation(Place, state);
}
