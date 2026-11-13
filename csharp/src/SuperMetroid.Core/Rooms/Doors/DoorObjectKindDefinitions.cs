using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room-object definitions that install coloured and grey doors, and the door each installs. A definition absent
/// here installs no coloured or grey door.
/// </summary>
internal static class DoorObjectKindDefinitions
{
    /// <summary>The colour and facing of the door each coloured-door definition installs.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, ColoredDoorVariant> ColoredDoors { get; } = new Dictionary<RoomObjectKind, ColoredDoorVariant>
    {
        [RoomObjectKind.YellowDoorFacingLeft] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left),
        [RoomObjectKind.YellowDoorFacingRight] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right),
        [RoomObjectKind.YellowDoorFacingUp] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up),
        [RoomObjectKind.YellowDoorFacingDown] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down),
        [RoomObjectKind.GreenDoorFacingLeft] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Left),
        [RoomObjectKind.GreenDoorFacingRight] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Right),
        [RoomObjectKind.GreenDoorFacingUp] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Up),
        [RoomObjectKind.GreenDoorFacingDown] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Down),
        [RoomObjectKind.RedDoorFacingLeft] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Left),
        [RoomObjectKind.RedDoorFacingRight] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Right),
        [RoomObjectKind.RedDoorFacingUp] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Up),
        [RoomObjectKind.RedDoorFacingDown] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Down),
    };

    /// <summary>
    /// The facing of the door each grey-door definition installs; Bomb Torizo's grey door faces right like the generic
    /// right-facing one.
    /// </summary>
    internal static IReadOnlyDictionary<RoomObjectKind, ColoredDoorOrientation> GreyDoors { get; } = new Dictionary<RoomObjectKind, ColoredDoorOrientation>
    {
        [RoomObjectKind.BombTorizoGreyDoor] = ColoredDoorOrientation.Right,
        [RoomObjectKind.GreyDoorFacingRight] = ColoredDoorOrientation.Right,
        [RoomObjectKind.GreyDoorFacingLeft] = ColoredDoorOrientation.Left,
        [RoomObjectKind.GreyDoorFacingUp] = ColoredDoorOrientation.Up,
        [RoomObjectKind.GreyDoorFacingDown] = ColoredDoorOrientation.Down,
    };
}
