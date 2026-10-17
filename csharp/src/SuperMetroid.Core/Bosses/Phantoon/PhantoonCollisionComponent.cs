namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One fixed collision component of a Phantoon BG2 frame, independent of its editable artwork.</summary>
/// <param name="Offset">The component's offset from the part's position.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order.</param>
internal readonly record struct PhantoonCollisionComponent(
    Offset16<RoomPixels> Offset, IReadOnlyList<PhantoonCollisionHitbox> Hitboxes);
