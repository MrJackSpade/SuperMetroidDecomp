namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One native physical component of a Crocomire body frame.</summary>
/// <param name="Offset">The component's offset from the body's position.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; empty for a component without collision.</param>
internal readonly record struct CrocomireBodyCollisionComponent(
    Offset16<RoomPixels> Offset, IReadOnlyList<CrocomireBodyCollisionHitbox> Hitboxes);
