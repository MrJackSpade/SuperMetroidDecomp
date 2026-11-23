namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// A Dragon body (#2175): it waits below the lava, rises to spit its fireballs at Samus and sinks again, moving the cosmetic
/// wing its room lists right after it (<see cref="DragonWingEntity"/>) and carrying each hit on to that wing.
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
/// <param name="followUpRecipient">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class DragonEntity(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    DragonEnemyState state, RoomEnemyPlace followUpRecipient)
    : DragonPartEntity(species, occupant, place, common, followUpRecipient), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Dragon species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the body occupies.</param>
    /// <param name="common">The shared participation.</param>
    internal DragonEntity(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : this(species, occupant, place, common, species.InitializeDragonBody(place), species.FollowUpRecipientOf(place))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DragonEnemyState State { get; } = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => Species.RunDragonMain(Occupant, Place, State, State.WingState, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DragonBehavior.AdvanceDragonBodyTypedAnimation(Place, State);
}
