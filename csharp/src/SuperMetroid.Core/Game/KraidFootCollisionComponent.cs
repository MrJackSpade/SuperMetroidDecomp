namespace SuperMetroid.Core.Game;

/// <summary>One foot part's room offset and shared collision-hitbox pointer.</summary>
internal readonly record struct KraidFootCollisionComponent(Offset16<RoomPixels> Offset, ushort HitboxPointer);
