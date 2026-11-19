namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy holding one place: bound when its identity is installed and discarded when the place changes occupant.
/// The behaviors it has are the participation interfaces it implements; the step keeps eligibility and order,
/// and asks the entity, which acts on the place it is bound to.
/// </summary>
internal interface IEnemyEntity
{
    /// <summary>The collision paths the enemy takes beyond the common ones.</summary>
    EnemyCapabilities Capabilities { get; }

    /// <summary>
    /// Sets up the enemy just installed in the place the entity is bound to (#2571), from the population load's
    /// surroundings, or from none for an enemy spawned on another enemy's behalf or rebuilt by a respawn. An enemy's
    /// initialization (<see cref="IEnemyInitialization"/>, <see cref="IEnemySurroundingsInitialization"/>) answers it, so
    /// every entity declares one.
    /// </summary>
    /// <param name="place">The place the enemy was installed in.</param>
    /// <param name="surroundings">The room, Samus, controller and camera a population load's spawn sees, or none.</param>
    void InitializeOccupant(RoomEnemyPlace place, EnemyPopulationSurroundings? surroundings);
}
