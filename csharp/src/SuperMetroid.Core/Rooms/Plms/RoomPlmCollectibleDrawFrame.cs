using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One native one-block collectible draw list and its physical level word.</summary>
internal readonly record struct RoomPlmCollectibleDrawFrame(
    int Pointer, RoomLevelWord LevelWord, string Id);
