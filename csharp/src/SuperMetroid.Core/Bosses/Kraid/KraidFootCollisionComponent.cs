using System.Collections.Immutable;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One foot part's room offset and the shared rectangle it collides with.</summary>
/// <param name="Offset">The component origin relative to Kraid's foot enemy, in room pixels.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; every foot component shares one.</param>
internal readonly record struct KraidFootCollisionComponent(Offset16<RoomPixels> Offset, ImmutableArray<KraidFootCollisionHitbox> Hitboxes);
