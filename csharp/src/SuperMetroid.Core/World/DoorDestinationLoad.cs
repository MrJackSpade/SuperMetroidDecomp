using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.World;

/// <summary>What a door transition's load of its destination leaves: the room it built and the source room's last view.</summary>
/// <param name="Destination">The destination room the load built, the loaded room from then on.</param>
/// <param name="SourceView">What the source room showed as the destination's load began, which the door's opening scroll keeps.</param>
[RoomScopeBoundary("A per-call result, returned by the destination's load and never kept.")]
internal readonly record struct DoorDestinationLoad(RoomScope Destination, DoorOpeningSourceView SourceView);
