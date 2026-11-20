namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An entry of a room's population bound to its species where the population is loaded (#2554): the population load builds
/// its enemy seeing the load's surroundings, which a species built the same way everywhere does not read.
/// </summary>
internal abstract class PopulationEnemySpawn : EnemySpawn
{
    /// <summary>Binds <paramref name="placement"/>.</summary>
    /// <param name="placement">The population entry.</param>
    private protected PopulationEnemySpawn(RoomEnemyPlacement placement)
        : base(placement)
    {
    }

    /// <summary>How the population load builds the entry's enemy in <paramref name="surroundings"/>.</summary>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
    internal abstract IEnemyBuild Loaded(EnemyPopulationSurroundings surroundings);
}
