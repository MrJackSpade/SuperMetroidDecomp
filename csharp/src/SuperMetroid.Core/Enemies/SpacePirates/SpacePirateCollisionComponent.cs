namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Engine-owned offset and hitbox list of one extended frame component.</summary>
/// <param name="Offset">Component origin relative to the Pirate's room position.</param>
/// <param name="Hitboxes">The component's authored collision rectangles, tested in order.</param>
internal readonly record struct SpacePirateCollisionComponent(
    Offset16<RoomPixels> Offset, IReadOnlyList<SpacePirateCollisionHitbox> Hitboxes);
