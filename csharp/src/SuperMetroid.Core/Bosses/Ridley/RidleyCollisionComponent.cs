namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One Ridley body component: its origin and the rectangles tested against it.</summary>
/// <param name="Offset">The component origin relative to Ridley's position, in room pixels.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; the first the probe overlaps wins.</param>
internal readonly record struct RidleyCollisionComponent(Offset16<RoomPixels> Offset, RidleyCollisionHitbox[] Hitboxes);
