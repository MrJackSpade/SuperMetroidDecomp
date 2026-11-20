namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The setup an occupant runs once after its identity is installed (#2571): its own initialization, or one that reads a
/// population load's surroundings. Its factory names it (<see cref="EnemyOccupant"/>), and the lifecycle runs it.
/// </summary>
internal abstract class EnemySetup
{
    /// <summary>Only the setups below exist.</summary>
    private EnemySetup()
    {
    }

    /// <summary>
    /// Runs the setup for the occupant just installed in <paramref name="place"/>, from the population load's
    /// <paramref name="surroundings"/>, or from none for an enemy spawned on another enemy's behalf or rebuilt by a respawn.
    /// </summary>
    /// <param name="place">The place the occupant was installed in.</param>
    /// <param name="surroundings">The room, Samus, controller and camera a population load's spawn sees, or none.</param>
    internal abstract void Run(RoomEnemyPlace place, EnemyPopulationSurroundings? surroundings);

    /// <summary>The occupant's own initialization, which reads neither its place nor its surroundings.</summary>
    /// <param name="entity">The entity, which sets itself up.</param>
    internal sealed class Own(IEnemyInitialization entity) : EnemySetup
    {
        /// <inheritdoc/>
        internal override void Run(RoomEnemyPlace place, EnemyPopulationSurroundings? surroundings) => entity.Initialize();
    }

    /// <summary>The occupant's initialization from a population load's surroundings.</summary>
    /// <param name="entity">The entity, which sets itself up from its surroundings.</param>
    internal sealed class FromSurroundings(IEnemySurroundingsInitialization entity) : EnemySetup
    {
        /// <inheritdoc/>
        /// <exception cref="InvalidDataException">The occupant was spawned on another enemy's behalf, with no surroundings.</exception>
        internal override void Run(RoomEnemyPlace place, EnemyPopulationSurroundings? surroundings) =>
            entity.Initialize(surroundings ?? throw new InvalidDataException(
                $"Enemy {Ensure.NotNull(place).DefinitionId} initializes from a population load's surroundings, but was spawned on another enemy's behalf."));
    }
}
