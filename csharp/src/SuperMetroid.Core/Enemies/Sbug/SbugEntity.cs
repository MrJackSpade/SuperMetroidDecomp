namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>One Sbug occupant: its own AI, with the shared reactions.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class SbugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, SbugSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal SbugSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal SbugEnemyState State { get; } = SbugBehavior.InitializeSbug(settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => SbugBehavior.RunSbugMainAi(Settings, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SbugBehavior.AdvanceSbugTypedAnimation(Place, State);
}
