using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A map, energy or missile station's access block, which a special block's reaction selects
/// (<see cref="BlockReactionDefinitions.StationAccesses"/>, #2484): the station it belongs to and the side of the station it
/// lies on. The save station's floor is a reaction of its own (<see cref="RoomBlockBehavior.IsSaveStationFloor"/>).
/// </summary>
/// <param name="Station">The station whose access block it is.</param>
/// <param name="Side">The side of the station the access block lies on.</param>
public readonly partial record struct StationAccess(StationKind Station, HorizontalDirection Side);
