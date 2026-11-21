namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// A Dragon body (#2175): it waits below the lava, rises to spit its fireballs at Samus and sinks again, moving the cosmetic
/// wing its room lists right after it (<see cref="DragonWingEntity"/>) and carrying each hit on to that wing.
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DragonEntity(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : DragonPartEntity(species, occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DragonEnemyState State { get; } = species.InitializeDragonBody(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => Species.RunDragonMain(Occupant, Place, State, State.WingState, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DragonBehavior.AdvanceDragonBodyTypedAnimation(Place, State);
}
