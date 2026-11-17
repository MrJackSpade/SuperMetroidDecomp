using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values and helpers of <see cref="GreyDoorObjects"/> that only verification reads.</summary>
internal static class GreyDoorObjectsConstants
{
    /// <summary>
    /// Maps the four grey doors and Bomb Torizo's right-facing grey door to their facing, each by its own kind (#1414).
    /// </summary>
    internal static bool TryIdentifyGreyDoor(
        RoomObjectKind kind,
        out ColoredDoorOrientation orientation) =>
        DoorObjectKindDefinitions.GreyDoors.TryGetValue(kind, out orientation);
}
