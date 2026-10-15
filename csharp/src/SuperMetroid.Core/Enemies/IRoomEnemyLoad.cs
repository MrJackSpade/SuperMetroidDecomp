using SuperMetroid.Core.Enemies.Gunship;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Everything <see cref="RoomEnemySystem.Load"/> needs: the population source and actor context its
/// child loaders read, the services the AI keeps for the room, and the load scenario it owns directly.
/// </summary>
public interface IRoomEnemyLoad : IRoomEnemyPopulationSource, IRoomEnemyActorContext
{
    /// <summary>The cartridge-state services the room's enemy AI calls.</summary>
    IRoomEnemyServices Services { get; }

    /// <summary>Which gunship arrival this load performs.</summary>
    GunshipLoadScenario GunshipScenario { get; }

    /// <summary>Whether camera reads made at load time are deferred because the room streams in through a door.</summary>
    bool DeferLoaderTimeCameraReads { get; }
}
