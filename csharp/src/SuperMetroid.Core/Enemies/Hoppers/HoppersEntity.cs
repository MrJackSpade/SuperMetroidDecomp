namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One Hoppers occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Hoppers species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class HoppersEntity(HoppersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, HopperSpawnSettings settings, HopperEnemyState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Hoppers species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    /// <param name="kind">The kind of hopper the occupant's definition is.</param>
    internal HoppersEntity(HoppersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        HopperSpawnSettings settings, HopperKind kind)
        : this(species, occupant, place, common, settings, species.InitializeHopper(place, settings, kind))
    {
    }

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal HopperSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal HopperEnemyState State { get; } = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunHoppersMainAi(Settings, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceHoppersTypedAnimation(Place, State);
}
