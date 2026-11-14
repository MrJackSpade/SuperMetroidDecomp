namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy whose initialization reads the spawn's surroundings: the room, Samus, the controller or the camera. An
/// entity implements this or <see cref="IEnemyInitialization"/>, never both.
/// </summary>
internal interface IEnemySurroundingsInitialization : IEnemyEntity
{
    /// <summary>Sets up the enemy just installed in the slot the entity is bound to, from its surroundings.</summary>
    /// <param name="surroundings">The room, Samus, controller and camera the spawn sees.</param>
    void Initialize(EnemySpawnSurroundings surroundings);
}
