namespace SuperMetroid.Core.Earthquakes;

/// <summary>
/// What an earthquake shakes: the room's blocks, the scenery behind them, and the enemies with their projectiles
/// (<see cref="EarthquakeShakeDefinitions"/>). Every earthquake shakes the blocks, the scenery or both; the enemies shake
/// only with the scenery.
/// </summary>
public enum EarthquakeReach
{
    /// <summary>The room's blocks alone.</summary>
    RoomBlocks,

    /// <summary>The room's blocks and the scenery behind them; the enemies stay still.</summary>
    RoomBlocksAndScenery,

    /// <summary>The room's blocks, the scenery, the enemies and their projectiles.</summary>
    RoomBlocksSceneryAndEnemies,

    /// <summary>The scenery, the enemies and their projectiles; the room's blocks stay still.</summary>
    SceneryAndEnemies,
}
