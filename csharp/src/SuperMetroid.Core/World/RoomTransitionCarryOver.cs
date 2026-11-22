using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// What a destination room reads from the room it replaces, captured before the destination's construction begins.
/// Nothing else may read the outgoing scope (#1329).
/// </summary>
/// <param name="PreviousCamera">
/// The outgoing room's camera, or null for the runtime's first room. A door load inherits its tracking state and both
/// fractions (#1266); the runtime's first room takes the fractions the previous game left
/// (<see cref="CameraFractionsKeptAcrossGames"/>).
/// </param>
[RoomScopeBoundary("Captures the outgoing room's parts the destination reads.")]
internal sealed partial record RoomTransitionCarryOver(ScrollBoundaryCamera? PreviousCamera);
