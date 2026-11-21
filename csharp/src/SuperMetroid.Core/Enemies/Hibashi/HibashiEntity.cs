namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>A Hibashi's visible flame: its rest, eruption and the collision part it moves, with the shared reactions.</summary>
/// <param name="species">The Hibashi species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class HibashiEntity(HibashiBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, HibashiFlameSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal HibashiFlameSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal HibashiEnemyState State { get; } = species.InitializeHibashi(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => HibashiBehavior.RunHibashiMainAi(Settings, Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceHibashiTypedAnimation(Place, State);
}
