namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy whose initialization reads the spawn's surroundings: the room, Samus, the controller or the camera. An
/// entity implements this or <see cref="IEnemyInitialization"/>, never both, and its factory hands it as an occupant with
/// this setup (<see cref="EnemyOccupant.Of(IEnemySurroundingsInitialization)"/>).
/// </summary>
internal interface IEnemySurroundingsInitialization : IEnemyEntity
{
    /// <summary>Sets up the enemy just installed in the place the entity is bound to, from its surroundings.</summary>
    /// <param name="surroundings">The room, Samus, controller and camera the spawn sees.</param>
    void Initialize(EnemyPopulationSurroundings surroundings);
}
