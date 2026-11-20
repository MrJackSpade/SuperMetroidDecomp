namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One Chozo statue: its own AI, with the shared reactions.</summary>
/// <param name="species">The Chozo statue species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the statue occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class ChozoStatueEntity(ChozoStatueBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, ChozoStatueSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal ChozoStatueSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal ChozoStatueState State { get; } = species.InitializeChozoStatue(occupant, settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ChozoStatueBehavior.RunChozoStatueMainAi(Occupant, Settings, Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceChozoStatueTypedAnimation(Place, State, context);
}
