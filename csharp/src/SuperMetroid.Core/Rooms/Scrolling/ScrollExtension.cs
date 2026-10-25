using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The collision type and offset a scroll extension block writes.</summary>
/// <param name="CollisionType">The collision type.</param>
/// <param name="Offset">The signed block offset the extension's BTS byte stores, -1 or 1.</param>
internal readonly record struct ScrollExtension(RoomCollisionType CollisionType, int Offset);
