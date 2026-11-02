using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>One one-block collectible draw, the terrain it writes and its artwork identity.</summary>
internal readonly record struct RoomPlmCollectibleDrawFrame(
    RoomPlmCollectibleDrawDefinitions.Draw Draw, RoomTerrainBlock Terrain, string Id);
