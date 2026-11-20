namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>One Sbug occupant: its own AI, with the shared reactions.</summary>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class SbugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, SbugSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal SbugEnemyState State { get; } = SbugBehavior.InitializeSbug(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => SbugBehavior.RunSbugMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SbugBehavior.AdvanceSbugTypedAnimation(Place, State, context);
}
