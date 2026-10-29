using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Everything <see cref="RoomEnemySystem.Load"/> needs besides the definitions and services its enemy system was built
/// with: the population source and actor context its child loaders read, and the load scenario it owns directly.
/// </summary>
public interface IRoomEnemyLoad : IRoomEnemyPopulationSource, IRoomEnemyActorContext
{
    /// <summary>Which gunship arrival this load performs.</summary>
    GunshipLoadScenario GunshipScenario { get; }

    /// <summary>Whether camera reads made at load time are deferred because the room streams in through a door.</summary>
    bool DeferLoaderTimeCameraReads { get; }

    /// <summary>What gameplay shows over the room's load (#1466), which its enemies show into.</summary>
    RoomAppearance Appearance { get; }

    /// <summary>The frame each of the room's animations shows (#1406), which its enemies select their pictures into.</summary>
    ArtworkFrameSelections ArtworkFrames { get; }
}
