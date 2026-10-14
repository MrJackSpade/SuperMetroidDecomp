namespace SuperMetroid.Core.Rooms;

/// <summary>One native one-block collectible draw list and its physical level word.</summary>
internal readonly record struct RoomPlmCollectibleDrawFrame(
    ushort Pointer, ushort LevelWord, string Id);
