namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One Cacatac occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Cacatac species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class CacatacEntity(CacatacBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, CacatacSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal CacatacEnemyState State { get; } = CacatacBehavior.InitializeCacatac(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunCacatacMain(Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceCacatacTypedAnimation(Place, State, context);
}
