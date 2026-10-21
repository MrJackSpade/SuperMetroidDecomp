using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The escape critters' shot reaction a block hands on to (#1395). The escape-animal PLM family implements it.</summary>
internal interface ICrittersEscapeReaction
{
    /// <summary>Runs $84:B978 for the block at <paramref name="blockIndex"/>, or returns false when the shot does not trigger it.</summary>
    bool TrySpawnCrittersEscapeReaction(RoomLevelData level, int blockIndex, SamusProjectileTypeWord projectileType);
}
