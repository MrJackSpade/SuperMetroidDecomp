namespace SuperMetroid.Core.Game;

/// <summary>A fixed bank-$A9 Mother Brain component offset and hitbox-list identity.</summary>
internal readonly record struct MotherBrainCollisionComponent(Offset16<RoomPixels> Offset, ushort HitboxPointer);
