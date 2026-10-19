namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One fixed physical component of a Draygon BG2 frame, independent of its replaceable art.</summary>
/// <param name="Offset">The component's offset from the body's position.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; empty while the body cannot be hit.</param>
internal readonly record struct DraygonCollisionComponent(
    Offset16<RoomPixels> Offset, IReadOnlyList<DraygonCollisionHitbox> Hitboxes);
