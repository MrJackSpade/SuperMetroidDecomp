namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Engine-owned component offset and bank-$A7 hitbox-list identity.</summary>
internal readonly record struct PhantoonCollisionComponent(Offset16<RoomPixels> Offset, ushort HitboxPointer);
