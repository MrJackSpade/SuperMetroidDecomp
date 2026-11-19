namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy whose initialization reads the spawn's surroundings: the room, Samus, the controller or the camera. An
/// entity implements this or <see cref="IEnemyInitialization"/>, never both.
/// </summary>
internal interface IEnemySurroundingsInitialization : IEnemyEntity
{
    /// <summary>Sets up the enemy just installed in the place the entity is bound to, from its surroundings.</summary>
    /// <param name="surroundings">The room, Samus, controller and camera the spawn sees.</param>
    void Initialize(EnemyPopulationSurroundings surroundings);

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The enemy was spawned on another enemy's behalf, with no surroundings.</exception>
    void IEnemyEntity.InitializeOccupant(RoomEnemyPlace place, EnemyPopulationSurroundings? surroundings) =>
        Initialize(surroundings ?? throw new InvalidDataException(
            $"Enemy {place.DefinitionId} initializes from a population load's surroundings, but was spawned on another enemy's behalf."));
}
