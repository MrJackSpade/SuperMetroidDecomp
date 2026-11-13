using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room-object definitions that install coloured and grey doors, and the door each installs (#1414: named by member,
/// replacing the ranges the room-object definitions implied). A definition absent here installs no
/// coloured or grey door.
/// </summary>
internal static class DoorObjectHeaderDefinitions
{
    /// <summary>The colour and facing of the door each coloured-door definition installs.</summary>
    internal static IReadOnlyDictionary<RoomObjectHeaderId, ColoredDoorVariant> ColoredDoors { get; } = new Dictionary<RoomObjectHeaderId, ColoredDoorVariant>
    {
        [RoomObjectHeaderId.YellowDoorFacingLeft] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left),
        [RoomObjectHeaderId.YellowDoorFacingRight] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right),
        [RoomObjectHeaderId.YellowDoorFacingUp] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up),
        [RoomObjectHeaderId.YellowDoorFacingDown] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down),
        [RoomObjectHeaderId.GreenDoorFacingLeft] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Left),
        [RoomObjectHeaderId.GreenDoorFacingRight] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Right),
        [RoomObjectHeaderId.GreenDoorFacingUp] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Up),
        [RoomObjectHeaderId.GreenDoorFacingDown] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Down),
        [RoomObjectHeaderId.RedDoorFacingLeft] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Left),
        [RoomObjectHeaderId.RedDoorFacingRight] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Right),
        [RoomObjectHeaderId.RedDoorFacingUp] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Up),
        [RoomObjectHeaderId.RedDoorFacingDown] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Down),
    };

    /// <summary>
    /// The facing of the door each grey-door definition installs; Bomb Torizo's grey door faces right like the generic
    /// right-facing one.
    /// </summary>
    internal static IReadOnlyDictionary<RoomObjectHeaderId, ColoredDoorOrientation> GreyDoors { get; } = new Dictionary<RoomObjectHeaderId, ColoredDoorOrientation>
    {
        [RoomObjectHeaderId.BombTorizoGreyDoor] = ColoredDoorOrientation.Right,
        [RoomObjectHeaderId.GreyDoorFacingRight] = ColoredDoorOrientation.Right,
        [RoomObjectHeaderId.GreyDoorFacingLeft] = ColoredDoorOrientation.Left,
        [RoomObjectHeaderId.GreyDoorFacingUp] = ColoredDoorOrientation.Up,
        [RoomObjectHeaderId.GreyDoorFacingDown] = ColoredDoorOrientation.Down,
    };
}
