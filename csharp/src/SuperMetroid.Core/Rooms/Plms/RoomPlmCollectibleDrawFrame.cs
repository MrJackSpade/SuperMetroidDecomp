using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One one-block collectible draw, its physical level word and its artwork identity.</summary>
internal readonly record struct RoomPlmCollectibleDrawFrame(
    RoomPlmCollectibleDrawDefinitions.Draw Draw, RoomLevelWord LevelWord, string Id);
