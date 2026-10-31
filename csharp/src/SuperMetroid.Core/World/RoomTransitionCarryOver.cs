using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

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
/// <param name="DoorGates">
/// The outgoing room's door-transition gates, which door-transition code owns for the whole transition; the
/// destination's enemies load with them clear and resume them once loaded (owner decision on #1329).
/// </param>
[RoomScopeBoundary("Captures the outgoing room's parts the destination reads.")]
internal sealed record RoomTransitionCarryOver(ScrollBoundaryCamera? PreviousCamera, FixedPosition<RoomPixels> InheritedLayer1Position,
    EnemyDoorTransitionGates DoorGates);
