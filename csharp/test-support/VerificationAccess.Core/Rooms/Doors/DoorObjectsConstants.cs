using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values and helpers of <see cref="DoorObjects"/> that only verification reads.</summary>
internal static class DoorObjectsConstants
{
    /// <summary>
    /// Names the color and facing of a colored-door room object: each of the twelve coloured doors by its own kind (#1414).
    /// </summary>
    internal static bool TryIdentifyColoredDoor(
        RoomObjectKind kind,
        out ColoredDoorColor color,
        out ColoredDoorOrientation orientation)
    {
        bool found = DoorObjectKindDefinitions.ColoredDoors.TryGetValue(kind, out ColoredDoorVariant variant);
        color = variant.Color;
        orientation = variant.Orientation;
        return found;
    }
}
