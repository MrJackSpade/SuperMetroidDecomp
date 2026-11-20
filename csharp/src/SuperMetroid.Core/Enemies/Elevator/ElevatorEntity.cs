namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>One Elevator occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Elevator species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ElevatorEntity(ElevatorBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemySurroundingsInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => species.SetUpElevator(Occupant, Place, surroundings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunElevatorMainAi(Occupant, Place, Place.RequireSpeciesState<ElevatorEnemyState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ElevatorBehavior.AdvanceElevatorTypedAnimation(Place, Place.RequireSpeciesState<ElevatorEnemyState>());
}
