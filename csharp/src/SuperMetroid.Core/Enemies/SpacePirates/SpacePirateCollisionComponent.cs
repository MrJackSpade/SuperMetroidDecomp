namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Engine-owned offset and hitbox identity in one extended frame component.</summary>
/// <param name="Offset">Component origin relative to the Pirate's room position.</param>
/// <param name="HitboxPointer">Bank-$B2 address of the authored collision list for this component.</param>
internal readonly record struct SpacePirateCollisionComponent(Offset16<RoomPixels> Offset, ushort HitboxPointer);
