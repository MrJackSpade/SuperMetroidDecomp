using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.World;

/// <summary>
/// What a door transition's destination load leaves (#2554): what the source room showed as the load began, which the
/// door's opening scroll keeps, and the loaded destination room.
/// </summary>
/// <param name="SourceView">What the source room showed as its destination's load began.</param>
/// <param name="Destination">The loaded destination room.</param>
[RoomLifetime]
internal sealed record DoorDestinationLoad(DoorOpeningSourceView SourceView, RoomScope Destination);
