namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned offset and hitbox-list identity of one Ridley body component.</summary>
internal readonly record struct RidleyCollisionComponent(Offset16<RoomPixels> Offset, ushort HitboxPointer);
