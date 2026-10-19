using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// What a destination room reads from the room it replaces, captured before the destination's construction begins.
/// Nothing else may read the outgoing scope (#1329, <c>docs/room-scope.md</c>).
/// </summary>
/// <param name="PreviousCamera">
/// The outgoing room's camera, or null for the runtime's first room. A door load inherits its tracking state and both
/// fraction words (<c>$090F</c>/<c>$0913</c>) (#1266).
/// </param>
/// <param name="InheritedLayer1Position">
/// The layer-1 position an earlier gameplay owner left in WRAM; the first room's camera takes its fraction words.
/// </param>
/// <param name="ProjectileResidue">The words the outgoing room's Samus projectile slots leave behind.</param>
internal sealed record RoomTransitionCarryOver(ScrollBoundaryCamera? PreviousCamera, FixedPosition<RoomPixels> InheritedLayer1Position,
    SamusProjectileResidue ProjectileResidue);
