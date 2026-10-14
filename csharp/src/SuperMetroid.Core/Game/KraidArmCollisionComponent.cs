namespace SuperMetroid.Core.Game;

/// <summary>One physical arm component of an extended spritemap.</summary>
/// <param name="Offset">The component origin relative to Kraid's arm enemy, in room pixels.</param>
/// <param name="HitboxPointer">The component's bank-$A7 hitbox list.</param>
internal readonly record struct KraidArmCollisionComponent(Offset16<RoomPixels> Offset, ushort HitboxPointer);
