using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One physical arm or foot component of an extended spritemap.</summary>
/// <param name="Offset">The component origin relative to Kraid's arm or foot enemy, in room pixels.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; the first the probe overlaps wins, and every foot component shares one.</param>
internal readonly record struct KraidCollisionComponent(Offset16<RoomPixels> Offset, ImmutableArray<KraidCollisionHitbox> Hitboxes);
