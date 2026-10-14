namespace SuperMetroid.Core.Game;

/// <summary>A fixed bank-$A5 component offset and hitbox-list identity, not editable art.</summary>
internal readonly record struct DraygonCollisionComponent(
    Offset16<RoomPixels> Offset, DraygonHitboxList HitboxPointer);
