namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>One Elevator occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Elevator species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class ElevatorEntity(ElevatorBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemyPopulationSurroundings surroundings, ElevatorSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The <c>surroundings</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
    private readonly EnemyPopulationSurroundings surroundings = surroundings;

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal ElevatorSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly ElevatorEnemyState state = species.InitializeElevator(settings, place, surroundings.Samus);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunElevatorMainAi(Settings, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ElevatorBehavior.AdvanceElevatorTypedAnimation(Place, state);
}
