namespace SuperMetroid.Core.Game;

/// <summary>One native physical component of a Crocomire body frame.</summary>
internal readonly record struct CrocomireBodyCollisionComponent(
    Offset16<RoomPixels> Offset, ushort HitboxPointer);
